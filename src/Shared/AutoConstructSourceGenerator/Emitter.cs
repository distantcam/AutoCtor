using Microsoft.CodeAnalysis;
using static AutoCtor.Diagnostics;

#if ROSLYN_3
using EmitterContext = Microsoft.CodeAnalysis.GeneratorExecutionContext;
#elif ROSLYN_4
using EmitterContext = Microsoft.CodeAnalysis.SourceProductionContext;
#endif

namespace AutoCtor;

[Generator(LanguageNames.CSharp)]
public partial class AutoConstructSourceGenerator
{
    private static class Emitter
    {
        public static void GenerateSource(
            EmitterContext context,
            (TypeModel Type, bool Guards) input)
        {
            var type = input.Type;
            var parameters = GetParameters(context, type, out var postCtorMethod);
            if (parameters == null)
                return;

            var source = new CodeBuilder()
                .AppendHeader()
                .AppendLine();

            using (source.StartPartialType(type))
            {
                source.AddGeneratedCodeAttribute();

                source.AppendIndent()
                    .Append($"public {type.Name}({parameters.CtorParameterDeclarations:commaindent})");
                if (parameters.HasBaseParameters)
                    source.Append($" : base({parameters.BaseParameters:commaindent})");
                source.AppendLine();

                using (source.StartBlock())
                {
                    foreach (var item in type.Fields.Concat(type.Properties))
                    {
                        var parameter = parameters.GetParameter(item);
                        if (parameter == null)
                            continue;

                        var addGuard =
                            ((type.Guard.HasValue && type.Guard.Value)
                                || (!type.Guard.HasValue && input.Guards))
                            && item.IsReferenceType
                            && !item.IsNullableAnnotated;

                        source.AppendIndent()
                            .Append($"{item.IdentifierName} = {parameter}");
                        if (addGuard)
                            source.Append($" ?? throw new global::System.ArgumentNullException(\"{parameter}\")");
                        source.Append(";")
                            .AppendLine();
                    }
                    if (postCtorMethod.HasValue)
                    {
                        source.AppendLine($"{postCtorMethod.Value.Name}({parameters.PostCtorParameters:commaindent});");
                    }
                }
            }

            context.AddSource($"{type.HintName}.g.cs", source);
        }

        public static ParameterList? GetParameters(
            EmitterContext? context,
            TypeModel type,
            out PostCtorModel? postCtorMethod)
        {
            postCtorMethod = GetPostCtorMethod(context, type);

            var parametersBuilder = new ParameterListBuilder(type.Fields, type.Properties);
            if (type.BaseCtorParameters != null)
            {
                parametersBuilder.SetBaseParameters(type.BaseCtorParameters);
            }
            else if (type.AutoConstructBase.Count == 1
                && GetParameters(null, type.AutoConstructBase[0], out _) is { } baseParameters)
            {
                parametersBuilder.SetBaseParameters(
                    type is { BaseTypeArguments: not null, BaseTypeParameters: not null }
                        ? SetGenerics(baseParameters, type.BaseTypeParameters.Value, type.BaseTypeArguments.Value)
                        : baseParameters);
            }
            if (postCtorMethod.HasValue)
            {
                parametersBuilder.SetPostCtorParameters(postCtorMethod.Value.Parameters);
            }
            var parameters = parametersBuilder.Build();

            return !parameters.Any() && !postCtorMethod.HasValue ? null : parameters;
        }

        private static List<ParameterModel> SetGenerics(
            ParameterList baseParameters,
            EquatableList<EquatableTypeSymbol> parameters,
            EquatableList<EquatableTypeSymbol> arguments)
        {
            var baseParameterList = new List<ParameterModel>();
            foreach (var bp in baseParameters)
            {
                baseParameterList.Add(new(
                    RefKind: RefKind.None,
                    Name: bp.Name,
                    ErrorName: bp.ErrorName,
                    KeyedService: bp.KeyedService,
                    HasExplicitDefaultValue: false,
                    ExplicitDefaultValue: string.Empty,
                    IsOutOrRef: false,
                    Locations: bp.Locations,
                    Type: new(SetGenerics(bp.Type.TypeSymbol, parameters, arguments))));
            }
            return baseParameterList;
        }

        private static ITypeSymbol FindTypeForArgument(
            ITypeSymbol type,
            EquatableList<EquatableTypeSymbol> parameters,
            EquatableList<EquatableTypeSymbol> arguments)
        {
            if (type is not ITypeParameterSymbol)
                return type;
            var eqType = new EquatableTypeSymbol(type);
            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] == eqType)
                {
                    return arguments[i].TypeSymbol;
                }
            }
            return type;
        }

        internal static ITypeSymbol SetGenerics(
            ITypeSymbol type,
            EquatableList<EquatableTypeSymbol> parameters,
            EquatableList<EquatableTypeSymbol> arguments)
        {
            if (type is ITypeParameterSymbol typeParam)
            {
                return FindTypeForArgument(typeParam, parameters, arguments);
            }

            if (type is INamedTypeSymbol namedType)
            {
                if (!namedType.IsGenericType)
                    return FindTypeForArgument(namedType, parameters, arguments);

                var typeArgs = namedType.TypeArguments
                    .Select(t => SetGenerics(t, parameters, arguments))
                    .ToArray();

                return namedType.ConstructedFrom.Construct(typeArgs);
            }

            return type;
        }

        private static PostCtorModel? GetPostCtorMethod(
            EmitterContext? context,
            TypeModel type)
        {
            var markedPostCtorMethods = type.PostCtorMethods;

            // ACTR001
            if (markedPostCtorMethods.Count > 1)
            {
                foreach (var m in markedPostCtorMethods)
                {
                    context?.ReportDiagnostic(m, ACTR001_AmbiguousMarkedPostConstructMethod);
                }
                return null;
            }

            if (markedPostCtorMethods.Count != 1)
                return null;

            var method = markedPostCtorMethods[0];
            var diagnosticReported = false;

            // ACTR002
            if (!method.ReturnsVoid)
            {
                context?.ReportDiagnostic(method, ACTR002_PostConstructMethodNotVoid);
                diagnosticReported = true;
            }

            // ACTR004
            if (method.IsGenericMethod)
            {
                context?.ReportDiagnostic(method, ACTR004_PostConstructMethodCannotBeGeneric);
                diagnosticReported = true;
            }

            foreach (var parameter in method.Parameters)
            {
                if (!parameter.IsOutOrRef)
                    continue;

                var matchingMember = type.Fields
                    .FirstOrDefault(m => m.Type == parameter.Type);

                // ACTR005
                if (parameter.KeyedService != null)
                {
                    context?.ReportDiagnostic(parameter, ACTR005_PostConstructOutParameterCannotBeKeyed);
                    diagnosticReported = true;
                }

                // ACTR006
                if (matchingMember != default && matchingMember.KeyedService != null)
                {
                    context?.ReportDiagnostic(matchingMember, ACTR006_PostConstructOutParameterMustNotMatchKeyedField);
                    // legacy: diagnostic reported but emitter still uses the method
                }

                // ACTR009
                if (matchingMember == default)
                {
                    context?.ReportDiagnostic(parameter, ACTR009_PostConstructOutParameterMustMatchMember);
                    diagnosticReported = true;
                }
            }

            return diagnosticReported ? null : method;
        }
    }
}

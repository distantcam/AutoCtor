using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using static AutoCtor.Diagnostics;

#if ROSLYN_3
using EmitterContext = Microsoft.CodeAnalysis.GeneratorExecutionContext;
#elif ROSLYN_4 || ROSLYN_5
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
            ((ImmutableArray<TypeModel> Types, ImmutableArray<PostCtorModel> PostCtorMethods) Models,
            bool Guards) input)
        {
            if (input.Models.Types.IsDefaultOrEmpty) return;

            var ctorMaps = new Dictionary<string, ParameterList>();
            var orderedTypes = input.Models.Types.OrderBy(static t => t.Depth);

            foreach (var type in orderedTypes)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                IEnumerable<ParameterModel>? baseParameters = default;

                if (type.HasBaseType)
                {
                    if (type is { BaseTypeArguments: not null, BaseTypeParameters: not null })
                    {
                        if (ctorMaps.TryGetValue(type.BaseTypeKey, out var temp))
                        {
                            var baseParameterList = new List<ParameterModel>();
                            foreach (var bp in temp)
                            {
                                var bpType = SetGenerics(
                                    bp.Type.TypeSymbol,
                                    type.BaseTypeParameters.Value,
                                    type.BaseTypeArguments.Value);

                                baseParameterList.Add(new(
                                    RefKind: RefKind.None,
                                    Name: bp.Name,
                                    ErrorName: bp.ErrorName,
                                    KeyedService: bp.KeyedService,
                                    HasExplicitDefaultValue: false,
                                    ExplicitDefaultValue: string.Empty,
                                    IsOutOrRef: false,
                                    Locations: bp.Locations,
                                    Type: new(bpType)));
                            }
                            baseParameters = baseParameterList;
                        }
                    }
                    else
                    {
                        ctorMaps.TryGetValue(type.BaseTypeKey, out var temp);
                        baseParameters = temp?.ToList();
                    }
                }

                var postCtorMethods = input.Models.PostCtorMethods
                    .Where(m => m.TypeKey == type.TypeKey)
                    .ToImmutableArray();

                var (source, parameters) = GenerateSource(context, type, postCtorMethods, baseParameters, input.Guards);

                if (source == null || parameters == null)
                    continue;

                ctorMaps.Add(type.TypeKey, parameters);

                context.AddSource($"{type.HintName}.g.cs", source);
            }
        }

        private static (SourceText?, ParameterList?) GenerateSource(
            EmitterContext context,
            TypeModel type,
            ImmutableArray<PostCtorModel> markedPostCtorMethods,
            IEnumerable<ParameterModel>? baseParameters,
            bool guards)
        {
            var postCtorMethod = GetPostCtorMethod(context, markedPostCtorMethods);

            var parametersBuilder = new ParameterListBuilder(type.Fields, type.Properties);
            if (type.HasBaseType)
            {
                if (type.BaseCtorParameters != null)
                {
                    parametersBuilder.SetBaseParameters(type.BaseCtorParameters);
                }
                else if (baseParameters != null)
                {
                    parametersBuilder.SetBaseParameters(baseParameters);
                }
            }
            if (postCtorMethod.HasValue)
            {
                parametersBuilder.SetPostCtorParameters(postCtorMethod.Value.Parameters);
            }
            var parameters = parametersBuilder.Build(context);

            if (!parameters.Any() && !postCtorMethod.HasValue)
                return (null, null);

            var source = new CodeBuilder()
                .AppendHeader()
                .AppendLine();

            using (source.StartPartialType(type))
            {
                source
                    .AddGeneratedAttributes(AttributeTargets.Method);

                source.AppendIndent()
                    .Append($"public {type.Name}({parameters.CtorParameterDeclarations:commaindent})")
                    .Append(parameters.HasBaseParameters,
                        $" : base({parameters.BaseParameters:commaindent})")
                    .AppendLine();

                using (source.StartBlock())
                {
                    foreach (var item in type.Fields.Concat(type.Properties))
                    {
                        var parameter = parameters.GetParameter(item);
                        if (parameter == null)
                            continue;

                        var addGuard =
                            ((type.Guard.HasValue && type.Guard.Value)
                                || (!type.Guard.HasValue && guards))
                            && item.IsReferenceType
                            && !item.IsNullableAnnotated;

                        source.AppendIndent()
                            .Append($"{item.IdentifierName} = {parameter}")
                            .Append(addGuard,
                                $" ?? throw new global::System.ArgumentNullException(\"{parameter}\")")
                            .Append(";")
                            .AppendLine();
                    }
                    if (postCtorMethod.HasValue)
                    {
                        source.AppendLine($"{postCtorMethod.Value.Name}({parameters.PostCtorParameters:commaindent});");
                    }
                }
            }

            return (source, parameters);
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

        private static ITypeSymbol SetGenerics(
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

                var typeArgs = namedType.TypeArguments.Select(t => SetGenerics(t, parameters, arguments)).ToArray();

                return namedType.ConstructedFrom.Construct(typeArgs);
            }

            return type;
        }

        private static PostCtorModel? GetPostCtorMethod(
            EmitterContext context,
            ImmutableArray<PostCtorModel> markedPostCtorMethods)
        {
            // ACTR001
            if (markedPostCtorMethods.Length > 1)
            {
                foreach (var m in markedPostCtorMethods)
                {
                    context.ReportDiagnostic(m, ACTR001_AmbiguousMarkedPostConstructMethod);
                }
                return null;
            }

            if (markedPostCtorMethods.Length != 1)
                return null;

            var method = markedPostCtorMethods[0];

            // ACTR002
            if (!method.ReturnsVoid)
            {
                context.ReportDiagnostic(method, ACTR002_PostConstructMethodNotVoid);
                return null;
            }

            // ACTR004
            if (method.IsGenericMethod)
            {
                context.ReportDiagnostic(method, ACTR004_PostConstructMethodCannotBeGeneric);
                return null;
            }

            foreach (var parameter in method.Parameters)
            {
                // ACTR005
                if (parameter.IsOutOrRef && parameter.KeyedService != null)
                {
                    context.ReportDiagnostic(parameter, ACTR005_PostConstructOutParameterCannotBeKeyed);
                    return null;
                }
            }

            return method;
        }
    }
}

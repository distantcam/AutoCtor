using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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
            (ImmutableArray<TypeModel> Types,
            ImmutableArray<PostCtorModel> PostCtorMethods,
            bool Guards,
            ImmutableArray<ServiceProviderModel> Providers,
            DuckTypes DuckTypes) input)
        {
            if (input.Types.IsDefaultOrEmpty && input.Providers.IsDefaultOrEmpty) return;

            var ctorMaps = new Dictionary<string, ParameterList>();
            var orderedTypes = input.Types.OrderBy(static t => t.Depth);

            // Indexed once rather than scanned per type: a marked method on every
            // [AutoConstruct] type in a large solution makes that scan quadratic.
            var postCtorsByType = new Dictionary<string, List<PostCtorModel>>();
            foreach (var method in input.PostCtorMethods)
            {
                if (!postCtorsByType.TryGetValue(method.TypeKey, out var marked))
                    postCtorsByType.Add(method.TypeKey, marked = []);
                marked.Add(method);
            }

            foreach (var type in orderedTypes)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                IEnumerable<ParameterModel>? baseParameters = default;

                if (type.HasBaseType)
                {
                    if (type is { BaseTypeArguments: not null, BaseTypeParameters: not null })
                    {
                        if (ctorMaps.TryGetValue(type.BaseTypeKey, out var paramList))
                        {
                            var baseParameterList = new List<ParameterModel>();
                            foreach (var bp in paramList)
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

                postCtorsByType.TryGetValue(type.TypeKey, out var postCtorMethods);

                var (source, parameters) = GenerateSource(context, type, postCtorMethods, baseParameters, input.Guards);

                if (source == null || parameters == null)
                    continue;

                ctorMaps.Add(type.TypeKey, parameters);

                context.AddSource($"{type.HintName}.g.cs", source);
            }

            // After the loop above, as ctorMaps holds the constructors AutoCtor generates.
            foreach (var provider in input.Providers)
                IoCEmitter.Generate(context, provider, ctorMaps, input.DuckTypes);
        }

        private static (SourceText?, ParameterList?) GenerateSource(
            EmitterContext context,
            TypeModel type,
            List<PostCtorModel>? markedPostCtorMethods,
            IEnumerable<ParameterModel>? baseParameters,
            bool guards)
        {
            var postCtorMethod = GetPostCtorMethod(context, type, markedPostCtorMethods);

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
                                || (!type.Guard.HasValue && guards))
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
            EmitterContext context,
            TypeModel type,
            List<PostCtorModel>? markedPostCtorMethods)
        {
            if (markedPostCtorMethods is null)
                return null;

            // ACTR001
            if (markedPostCtorMethods.Count > 1)
            {
                foreach (var m in markedPostCtorMethods)
                {
                    context.ReportDiagnostic(m, ACTR001_AmbiguousMarkedPostConstructMethod);
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
                context.ReportDiagnostic(method, ACTR002_PostConstructMethodNotVoid);
                diagnosticReported = true;
            }

            // ACTR004
            if (method.IsGenericMethod)
            {
                context.ReportDiagnostic(method, ACTR004_PostConstructMethodCannotBeGeneric);
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
                    context.ReportDiagnostic(parameter, ACTR005_PostConstructOutParameterCannotBeKeyed);
                    diagnosticReported = true;
                }

                // ACTR006
                if (matchingMember != default && matchingMember.KeyedService != null)
                {
                    context.ReportDiagnostic(matchingMember, ACTR006_PostConstructOutParameterMustNotMatchKeyedField);
                    // legacy: diagnostic reported but emitter still uses the method
                }

                // ACTR009
                if (matchingMember == default)
                {
                    context.ReportDiagnostic(parameter, ACTR009_PostConstructOutParameterMustMatchMember);
                    diagnosticReported = true;
                }
            }

            return diagnosticReported ? null : method;
        }
    }
}

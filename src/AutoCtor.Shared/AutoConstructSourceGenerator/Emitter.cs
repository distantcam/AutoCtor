using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

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

                                baseParameterList.Add(new(bp.Name, new(bpType)));
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
                    .ToList();

                var (source, parameters) = GenerateSource(context, type, postCtorMethods, baseParameters, input.Guards);

                if (source == null || parameters == null)
                    return;

                ctorMaps.Add(type.TypeKey, parameters);

                context.AddSource($"{type.HintName}.g.cs", source);
            }
        }

        private static (SourceText?, ParameterList?) GenerateSource(
            EmitterContext context,
            TypeModel type,
            IEnumerable<PostCtorModel> markedPostCtorMethods,
            IEnumerable<ParameterModel>? baseParameters,
            bool guards)
        {
            var postCtorMethod = GetPostCtorMethod(context, markedPostCtorMethods);

            var parameters = new ParameterList(type.Fields, type.Properties);
            if (type.HasBaseType)
            {
                if (type.BaseCtorParameters != null)
                {
                    parameters.AddParameters(type.BaseCtorParameters);
                }
                else if (baseParameters != null)
                {
                    parameters.AddParameters(baseParameters);
                }
            }
            if (postCtorMethod.HasValue)
            {
                parameters.AddPostCtorParameters(postCtorMethod.Value.Parameters);
            }
            parameters.MakeUniqueNames();

            if (!parameters.Any())
                return (null, null);

            var source = new CodeBuilder()
                .AppendHeader()
                .AppendLine();

            using (source.StartPartialType(type))
            {
                source
                    .AddCompilerGeneratedAttribute()
                    .AddGeneratedCodeAttribute()
                    .AddDebuggerNonUserCodeAttribute();

                source.AppendIndent()
                    .Append($"public {type.Name}({parameters})")
                    .Append(parameters.HasBaseParameters, $" : base({parameters:B})")
                    .AppendLine();

                using (source.StartBlock())
                {
                    foreach (var item in type.Fields.Concat(type.Properties))
                    {
                        var parameter = parameters.ParameterName(item);

                        var addGuard =
                            ((type.Guard.HasValue && type.Guard.Value)
                                || (!type.Guard.HasValue && guards))
                            && item.IsReferenceType
                            && !item.IsNullableAnnotated;

                        source.AppendIndent()
                            .Append($"this.{item.IdentifierName} = {parameter}")
                            .Append(addGuard, $" ?? throw new global::System.ArgumentNullException(\"{parameter}\")")
                            .Append(";")
                            .AppendLine();
                    }
                    if (postCtorMethod.HasValue)
                    {
                        source.AppendLine($"{postCtorMethod.Value.Name}({parameters:P});");
                    }
                }
            }

            return (source, parameters);
        }

        private static ITypeSymbol FindTypeForArgument(ITypeSymbol type, EquatableList<EquatableTypeSymbol> parameters, EquatableList<EquatableTypeSymbol> arguments)
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

        private static ITypeSymbol SetGenerics(ITypeSymbol type, EquatableList<EquatableTypeSymbol> parameters, EquatableList<EquatableTypeSymbol> arguments)
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

        private static void ReportDiagnostic(EmitterContext context, PostCtorModel method, DiagnosticDescriptor diagnostic)
        {
            foreach (var loc in method.Locations)
                context.ReportDiagnostic(Diagnostic.Create(diagnostic, loc, method.ErrorName));
        }

        private static PostCtorModel? GetPostCtorMethod(
            EmitterContext context,
            IEnumerable<PostCtorModel> markedPostCtorMethods)
        {
            // ACTR001
            if (markedPostCtorMethods.MoreThan(1))
            {
                foreach (var m in markedPostCtorMethods)
                {
                    ReportDiagnostic(context, m, Diagnostics.AmbiguousMarkedPostConstructMethodWarning);
                }
                return null;
            }

            if (!markedPostCtorMethods.Any())
                return null;

            var method = markedPostCtorMethods.First();

            // ACTR002
            if (!method.ReturnsVoid)
            {
                ReportDiagnostic(context, method, Diagnostics.PostConstructMethodNotVoidWarning);
                return null;
            }

            // ACTR003
            if (method.HasOptionalParameters)
            {
                ReportDiagnostic(context, method, Diagnostics.PostConstructMethodHasOptionalArgsWarning);
                return null;
            }

            // ACTR004
            if (method.IsGenericMethod)
            {
                ReportDiagnostic(context, method, Diagnostics.PostConstructMethodCannotBeGenericWarning);
                return null;
            }

            return method;
        }
    }
}

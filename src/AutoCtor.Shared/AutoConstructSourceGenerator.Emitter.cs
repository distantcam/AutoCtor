using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.SymbolDisplayFormat;

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

                IEnumerable<Parameter>? baseParameters = default;

                if (type.HasBaseType)
                {
                    if (type is { BaseTypeArguments: not null, BaseTypeParameters: not null })
                    {
                        if (ctorMaps.TryGetValue(type.BaseTypeKey, out var temp))
                        {
                            var baseParameterList = new List<Parameter>();
                            foreach (var bp in temp)
                            {
                                var bpName = bp.Name;
                                var bpType = bp.Type;
                                for (var i = 0; i < type.BaseTypeParameters.Value.Count; i++)
                                {
                                    if (SymbolEqualityComparer.Default.Equals(type.BaseTypeParameters.Value[i], bp.Type))
                                    {
                                        bpType = type.BaseTypeArguments.Value[i];
                                        break;
                                    }
                                }
                                baseParameterList.Add(new Parameter(bpType, bpName));
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
                    .Where(m => TypeModel.CreateKey(m.Method.ContainingType) == type.TypeKey)
                    .ToList();

                var (source, parameters) = GenerateSource(context, type, postCtorMethods.Select(m => m.Method), baseParameters, input.Guards);

                ctorMaps.Add(type.TypeKey, parameters);

                context.AddSource($"{type.HintName}.g.cs", source);
            }
        }

        private static (SourceText, ParameterList) GenerateSource(
            EmitterContext context,
            TypeModel type,
            IEnumerable<IMethodSymbol> markedPostCtorMethods,
            IEnumerable<Parameter>? baseParameters,
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
                    parameters.AddBaseParameters(baseParameters);
                }
            }
            if (postCtorMethod != null)
            {
                parameters.AddPostCtorParameters(postCtorMethod.Parameters);
            }
            parameters.MakeUniqueNames();

            var source = new CodeBuilder()
                .AppendHeader()
                .AppendLine();

            using (source.StartPartialType(type))
            {
                source.AddCompilerGeneratedAttribute().AddGeneratedCodeAttribute();

                if (parameters.HasBaseParameters)
                {
                    source.AppendLine($"public {type.Name}({parameters.ToParameterString()}) : base({parameters.ToBaseParameterString()})");
                }
                else
                {
                    source.AppendLine($"public {type.Name}({parameters.ToParameterString()})");
                }

                using (source.StartBlock())
                {
                    var items = type.Fields
                        .Select(f => (f.Type.IsReferenceType, Name: f.Name.EscapeKeywordIdentifier(), Parameter: parameters.ParameterName(f)))
                        .Concat(type.Properties
                        .Select(p => (p.Type.IsReferenceType, Name: p.Name.EscapeKeywordIdentifier(), Parameter: parameters.ParameterName(p))));

                    foreach (var item in items)
                    {
                        if (((type.Guard.HasValue && type.Guard.Value) ||
                            (!type.Guard.HasValue && guards)) &&
                            item.IsReferenceType)
                            source.AppendLine(
$"this.{item.Name} = {item.Parameter} ?? throw new global::System.ArgumentNullException(\"{item.Parameter}\");"
                            );
                        else
                            source.AppendLine(
$"this.{item.Name} = {item.Parameter};"
                            );
                    }
                    if (postCtorMethod != null)
                    {
                        source.AppendLine($"{postCtorMethod.Name}({parameters.ToPostCtorParameterString()});");
                    }
                }
            }

            return (source, parameters);
        }

        private static IMethodSymbol? GetPostCtorMethod(
            EmitterContext context,
            IEnumerable<IMethodSymbol> markedPostCtorMethods)
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
            if (method.Parameters.Any(static p => p.IsOptional))
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

        private static void ReportDiagnostic(EmitterContext context, IMethodSymbol method, DiagnosticDescriptor diagnostic)
        {
            foreach (var loc in method.Locations)
                context.ReportDiagnostic(Diagnostic.Create(diagnostic, loc, method.ToDisplayString(CSharpShortErrorMessageFormat)));
        }
    }
}

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AutoCtor;

public partial class AutoConstructSourceGenerator
{
    private static class Emitter
    {
        public static void GenerateSource(
#if ROSLYN_3_11
            GeneratorExecutionContext context,
#elif ROSLYN_4_0 || ROSLYN_4_4
            SourceProductionContext context,
#endif
            ((ImmutableArray<TypeModel> Types,
            ImmutableArray<IMethodSymbol> PostCtorMethods) Models, bool Guards) input)
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
                    if (type.BaseTypeArguments.HasValue && type.BaseTypeParameters.HasValue)
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
                    .Where(m => TypeModel.CreateKey(m.ContainingType) == type.TypeKey)
                    .ToList();

                (var source, var parameters) = GenerateSource(context, type, postCtorMethods, baseParameters, input.Guards);

                ctorMaps.Add(type.TypeKey, parameters);

                context.AddSource($"{type.HintName}.g.cs", source);
            }
        }

        private static (SourceText, ParameterList) GenerateSource(
#if ROSLYN_3_11
            GeneratorExecutionContext context,
#elif ROSLYN_4_0 || ROSLYN_4_4
            SourceProductionContext context,
#endif
            TypeModel type,
            IEnumerable<IMethodSymbol> markedPostCtorMethods,
            IEnumerable<Parameter>? baseParameters,
            bool guards)
        {
            var postCtorMethod = GetPostCtorMethod(context, markedPostCtorMethods);

            var parameters = new ParameterList(type.Fields);
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
                    foreach (var f in type.Fields)
                    {
                        if (((type.Guard.HasValue && type.Guard.Value) ||
                            (!type.Guard.HasValue && guards)) &&
                            f.Type.IsReferenceType)
                            source.AppendLine(
$"this.{f.Name.EscapeKeywordIdentifier()} = {parameters.FieldParameterName(f)} ?? throw new global::System.ArgumentNullException(\"{parameters.FieldParameterName(f)}\");"
                            );
                        else
                            source.AppendLine(
$"this.{f.Name.EscapeKeywordIdentifier()} = {parameters.FieldParameterName(f)};"
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
#if ROSLYN_3_11
            GeneratorExecutionContext context,
#elif ROSLYN_4_0 || ROSLYN_4_4
            SourceProductionContext context,
#endif
            IEnumerable<IMethodSymbol> markedPostCtorMethods)
        {
            // ACTR001
            if (markedPostCtorMethods.MoreThan(1))
            {
                foreach (var loc in markedPostCtorMethods.SelectMany(static m => m.Locations))
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.AmbiguousMarkedPostConstructMethodWarning, loc));
                return null;
            }

            if (!markedPostCtorMethods.Any())
                return null;

            var method = markedPostCtorMethods.First();

            // ACTR002
            if (!method.ReturnsVoid)
            {
                foreach (var loc in method.Locations)
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.PostConstructMethodNotVoidWarning, loc, method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
                return null;
            }

            // ACTR003
            if (method.Parameters.Any(static p => p.IsOptional))
            {
                foreach (var loc in method.Locations)
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.PostConstructMethodHasOptionalArgsWarning, loc, method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
                return null;
            }

            // ACTR004
            if (method.IsGenericMethod)
            {
                foreach (var loc in method.Locations)
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.PostConstructMethodCannotBeGenericWarning, loc, method.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
                return null;
            }

            return method;
        }
    }
}

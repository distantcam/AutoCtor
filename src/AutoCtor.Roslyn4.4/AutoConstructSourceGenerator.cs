using Microsoft.CodeAnalysis;

namespace AutoCtor;

[Generator(LanguageNames.CSharp)]
public sealed partial class AutoConstructSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var properties = context.AnalyzerConfigOptionsProvider
            .Select(static (p, ct) =>
            {
                return p.GlobalOptions.TryGetValue("build_property.AutoCtorGuards", out var value)
                    && (value.Equals("true", StringComparison.OrdinalIgnoreCase)
                    || value.Equals("enable", StringComparison.OrdinalIgnoreCase));
            });

        var types = context.SyntaxProvider.ForAttributeWithMetadataName(
            Parser.AutoConstructAttributeFullName,
            Parser.IsTypeDeclaration,
            static (c, ct) => TypeModel.Create((INamedTypeSymbol)c.TargetSymbol))
        .Collect();

        var postCtorMethods = context.SyntaxProvider.ForAttributeWithMetadataName(
            Parser.AutoPostConstructAttributeFullName,
            Parser.IsMethodDeclaration,
            static (c, ct) => (IMethodSymbol)c.TargetSymbol)
        .Collect();

        context.RegisterSourceOutput(
            types.Combine(postCtorMethods).Combine(properties),
            Emitter.GenerateSource);

        context.RegisterPostInitializationOutput(static c =>
            c.AddSource(AttributeEmitter.HintName, AttributeEmitter.GenerateSource()));
    }
}

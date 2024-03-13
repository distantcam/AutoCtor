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

        var types = context.SyntaxProvider.CreateSyntaxProvider(
            Parser.IsTypeDeclaration,
            Parser.GetMarkedNamedTypeSymbol)
            .Where(static s => s is not null)
            .Select(static (s, ct) => TypeModel.Create(s!))
            .Collect();

        var postCtorMethods = context.SyntaxProvider.CreateSyntaxProvider(
            Parser.IsMethodDeclaration,
            Parser.GetMarkedMethodSymbol)
            .Where(static s => s is not null)
            .Select(static (s, ct) => s!)
            .Collect();

        context.RegisterSourceOutput(
            types.Combine(postCtorMethods).Combine(properties),
            Emitter.GenerateSource);

        context.RegisterPostInitializationOutput(static c =>
            c.AddSource(AttributeEmitter.HintName, AttributeEmitter.GenerateSource()));
    }
}

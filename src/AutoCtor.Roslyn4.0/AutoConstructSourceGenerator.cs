using Microsoft.CodeAnalysis;

namespace AutoCtor;

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
            GeneratorUtilities.IsTypeDeclarationWithAttributes,
            GeneratorUtilities.GetSymbol<INamedTypeSymbol>)
        .Where(static x => GeneratorUtilities.HasAttribute(x, AttributeNames.AutoConstruct))
        .Select(static (x, _) => TypeModel.Create(x!))
        .Collect();

        var postCtorMethods = context.SyntaxProvider.CreateSyntaxProvider(
            GeneratorUtilities.IsMethodDeclarationWithAttributes,
            GeneratorUtilities.GetSymbol<IMethodSymbol>)
        .Where(static x => GeneratorUtilities.HasAttribute(x, AttributeNames.AutoPostConstruct))
        .Select(static (x, _) => PostCtorModel.Create(x!))
        .Collect();

        context.RegisterSourceOutput(
            types.Combine(postCtorMethods).Combine(properties),
            Emitter.GenerateSource);
    }
}

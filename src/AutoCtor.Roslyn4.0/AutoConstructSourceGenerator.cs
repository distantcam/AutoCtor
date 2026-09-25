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
            GeneratorUtilities.GetPrimarySymbol<INamedTypeSymbol>)
        .Where(static x => Utilities.HasAttribute(x, AttributeNames.AutoConstruct))
        .Select(static (x, _) => TypeModel.Create(x!))
        .Collect();

        var postCtorMethods = context.SyntaxProvider.CreateSyntaxProvider(
            GeneratorUtilities.IsMethodDeclarationWithAttributes,
            GeneratorUtilities.GetSymbol<IMethodSymbol>)
        .Where(static x => Utilities.HasAttribute(x, AttributeNames.AutoPostConstruct))
        .Select(static (x, _) => PostCtorModel.Create(x!))
        .Collect();

        var serviceProviders = context.SyntaxProvider.CreateSyntaxProvider(
            GeneratorUtilities.IsTypeDeclarationWithAttributes,
            GeneratorUtilities.GetPrimarySymbol<INamedTypeSymbol>)
        .Where(static x => Utilities.HasAttribute(x, AttributeNames.ServiceProvider))
        .Select(static (x, _) => ServiceProviderModel.Create(x!))
        .Collect();

        var duckTypes = context.CompilationProvider.Select(static (c, ct) => DuckTypes.Create(c));

        context.RegisterSourceOutput(
            types.Combine(postCtorMethods).Combine(properties).Combine(serviceProviders).Combine(duckTypes)
                .Select(static (x, _) => (x.Left.Left.Left.Left, x.Left.Left.Left.Right, x.Left.Left.Right, x.Left.Right, x.Right)),
            Emitter.GenerateSource);
    }
}

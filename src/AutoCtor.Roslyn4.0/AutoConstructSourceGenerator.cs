using Microsoft.CodeAnalysis;

namespace AutoCtor;

public sealed partial class AutoConstructSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var properties = context.AnalyzerConfigOptionsProvider
        .Select(static (p, _) =>
        {
            return p.GlobalOptions.TryGetValue("build_property.AutoCtorGuards", out var value)
                && (value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("enable", StringComparison.OrdinalIgnoreCase));
        });

        var types = context.SyntaxProvider.CreateSyntaxProvider(
            GeneratorUtilities.IsTypeDeclarationWithAttributes,
            GeneratorUtilities.GetPrimarySymbol<INamedTypeSymbol>)
        .Where(static x => Utilities.HasAttribute(x, AttributeNames.AutoConstructName))
        .Select(static (x, _) => TypeModel.Create(x!));

        var serviceProviders = context.SyntaxProvider.CreateSyntaxProvider(
            GeneratorUtilities.IsTypeDeclarationWithAttributes,
            GeneratorUtilities.GetPrimarySymbol<INamedTypeSymbol>)
        .Where(static x => Utilities.HasAttribute(x, AttributeNames.ServiceProviderName))
        .Select(static (x, _) => ServiceProviderModel.Create(x!))
        .Collect();

        var duckTypes = context.CompilationProvider.Select(static (c, ct) => DuckTypes.Create(c));

        context.RegisterSourceOutput(types.Combine(properties), Emitter.GenerateSource);

        context.RegisterSourceOutput(
            serviceProviders.Combine(types.Collect()).Combine(duckTypes)
                .Select(static (x, _) => (x.Left.Left, x.Left.Right, x.Right)),
            IoCEmitter.GenerateProviders);
    }
}

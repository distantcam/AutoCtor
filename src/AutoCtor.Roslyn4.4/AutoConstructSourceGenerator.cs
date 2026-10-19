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
        })
        .WithTrackingName(TrackingNames.BuildProperties);

        var types = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeNames.AutoConstruct,
            GeneratorUtilities.IsTypeDeclarationWithAttributes,
            static (c, _) => TypeModel.Create((INamedTypeSymbol)c.TargetSymbol))
        .WithTrackingName(TrackingNames.TypeModels);

        var serviceProviders = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeNames.ServiceProvider,
            GeneratorUtilities.IsTypeDeclarationWithAttributes,
            static (c, _) => ServiceProviderModel.Create((INamedTypeSymbol)c.TargetSymbol))
        .WithTrackingName(TrackingNames.ServiceProviders)
        .Collect();

        var duckTypes = context.CompilationProvider.Select(static (c, _) => DuckTypes.Create(c));

        context.RegisterSourceOutput(types.Combine(properties), Emitter.GenerateSource);

        context.RegisterSourceOutput(
            serviceProviders.Combine(types.Collect()).Combine(duckTypes)
                .Select(static (x, _) => (x.Left.Left, x.Left.Right, x.Right)),
            IoCEmitter.GenerateProviders);
    }
}

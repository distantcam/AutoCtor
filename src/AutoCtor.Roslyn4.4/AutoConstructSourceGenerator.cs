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
        })
        .WithTrackingName(TrackingNames.BuildProperties);

        var types = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeNames.AutoConstruct,
            GeneratorUtilities.IsTypeDeclarationWithAttributes,
            static (c, ct) => TypeModel.Create((INamedTypeSymbol)c.TargetSymbol))
        .WithTrackingName(TrackingNames.TypeModels)
        .Collect();

        var postCtorMethods = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeNames.AutoPostConstruct,
            GeneratorUtilities.IsMethodDeclarationWithAttributes,
            static (c, ct) => PostCtorModel.Create((IMethodSymbol)c.TargetSymbol))
        .WithTrackingName(TrackingNames.PostCtorMethods)
        .Collect();

        var serviceProviders = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeNames.ServiceProvider,
            GeneratorUtilities.IsTypeDeclarationWithAttributes,
            static (c, ct) => ServiceProviderModel.Create((INamedTypeSymbol)c.TargetSymbol))
        .WithTrackingName(TrackingNames.ServiceProviders)
        .Collect();

        var duckTypes = context.CompilationProvider.Select(static (c, ct) => DuckTypes.Create(c));

        context.RegisterSourceOutput(
            types.Combine(postCtorMethods).Combine(properties).Combine(serviceProviders).Combine(duckTypes)
                .Select(static (x, _) => (x.Left.Left.Left.Left, x.Left.Left.Left.Right, x.Left.Left.Right, x.Left.Right, x.Right)),
            Emitter.GenerateSource);
    }
}

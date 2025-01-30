using Microsoft.CodeAnalysis;

namespace AutoCtor;

public sealed partial class AttributeSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static c =>
            c.AddSource(Emitter.HintName, Emitter.GenerateSource()));
    }
}

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
            Parser.AutoConstructAttributeFullName,
            GeneratorUtilities.IsTypeDeclaration,
            static (c, ct) => TypeModel.Create((INamedTypeSymbol)c.TargetSymbol))
        .WithTrackingName(TrackingNames.TypeModels)
        .Collect();

        var postCtorMethods = context.SyntaxProvider.ForAttributeWithMetadataName(
            Parser.AutoPostConstructAttributeFullName,
            GeneratorUtilities.IsMethodDeclaration,
            static (c, ct) => PostCtorModel.Create((IMethodSymbol)c.TargetSymbol))
        .WithTrackingName(TrackingNames.PostCtorMethods)
        .Collect();

        context.RegisterSourceOutput(
            types.Combine(postCtorMethods).Combine(properties),
            Emitter.GenerateSource);
    }
}

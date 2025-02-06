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
        });

        var types = context.SyntaxProvider.CreateSyntaxProvider(
            static (n, ct) => GeneratorUtilities.IsTypeDeclaration(n, ct),
            GetTypeModel)
        .Where(static x => x.HasValue)
        .Select(static (x, _) => x!.Value)
        .Collect();

        var postCtorMethods = context.SyntaxProvider.CreateSyntaxProvider(
            static (n, ct) => GeneratorUtilities.IsMethodDeclaration(n, ct),
            GetPostCtorModel)
        .Where(static x => x.HasValue)
        .Select(static (x, _) => x!.Value)
        .Collect();

        context.RegisterSourceOutput(
            types.Combine(postCtorMethods).Combine(properties),
            Emitter.GenerateSource);
    }

    private static TypeModel? GetTypeModel(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.SemanticModel
            .GetDeclaredSymbol(context.Node, cancellationToken) is not INamedTypeSymbol type)
            return null;

        foreach (var attr in type.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() == AttributeNames.AutoConstruct)
                return TypeModel.Create(type);
        }

        return null;
    }

    private static PostCtorModel? GetPostCtorModel(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.SemanticModel
            .GetDeclaredSymbol(context.Node, cancellationToken) is not IMethodSymbol method)
            return null;

        foreach (var attr in method.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() == AttributeNames.AutoPostConstruct)
                return PostCtorModel.Create(method);
        }

        return null;
    }
}

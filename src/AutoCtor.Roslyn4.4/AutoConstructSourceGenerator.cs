using AutoCtor.Models;
using Microsoft.CodeAnalysis;

namespace AutoCtor;

[Generator(LanguageNames.CSharp)]
public sealed partial class AutoConstructSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var types = context.SyntaxProvider.ForAttributeWithMetadataName(
            Parser.AutoConstructAttributeFullName,
            Parser.IsTypeDeclaration,
            static (c, ct) => new TypeModel((INamedTypeSymbol)c.TargetSymbol))
        .Collect();

        var postCtorMethods = context.SyntaxProvider.ForAttributeWithMetadataName(
            Parser.AutoPostConstructAttributeFullName,
            Parser.IsMethodDeclaration,
            static (c, ct) => (IMethodSymbol)c.TargetSymbol)
        .Collect();

        var contextForGenerator = types.Combine(postCtorMethods);

        context.RegisterSourceOutput(types.Combine(postCtorMethods), Emitter.GenerateSource);

        context.RegisterPostInitializationOutput(static c =>
            c.AddSource(AttributeEmitter.HintName, AttributeEmitter.GenerateSource()));
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoCtor;

public partial class AutoConstructSourceGenerator
{
    private static class Parser
    {
        public static INamedTypeSymbol? GetMarkedNamedTypeSymbol(
            GeneratorSyntaxContext context,
            CancellationToken cancellationToken)
        {
            if (!MemberHasAttribute(AttributeNames.AutoConstruct, context, cancellationToken))
                return null;
            return context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) as INamedTypeSymbol;
        }

        public static IMethodSymbol? GetMarkedMethodSymbol(
            GeneratorSyntaxContext context,
            CancellationToken cancellationToken)
        {
            if (!MemberHasAttribute(AttributeNames.AutoPostConstruct, context, cancellationToken))
                return null;
            return context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken) as IMethodSymbol;
        }

        private static bool MemberHasAttribute(
            string attribute,
            GeneratorSyntaxContext context,
            CancellationToken cancellationToken)
        {
            foreach (var attributeListSyntax in ((MemberDeclarationSyntax)context.Node).AttributeLists)
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax, cancellationToken)
                        .Symbol is not IMethodSymbol attributeSymbol)
                        continue;

                    var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    var fullName = attributeContainingTypeSymbol.ToDisplayString();

                    if (fullName != attribute) continue;

                    return true;
                }
            return false;
        }
    }
}

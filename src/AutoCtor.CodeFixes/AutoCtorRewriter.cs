using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoCtor.CodeFixes;

internal sealed class AutoCtorRewriter(
    TypeDeclarationSyntax typeDeclaration,
    ConstructorDeclarationSyntax? ctorDeclaration
) : CSharpSyntaxRewriter
{
    // The target type and all containing types – the only types that need 'partial'.
    private readonly ImmutableArray<BaseTypeDeclarationSyntax> _targetAndAncestors =
        typeDeclaration
            .AncestorsAndSelf()
            .OfType<BaseTypeDeclarationSyntax>()
            .ToImmutableArray();

    public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
    {
        node = (CompilationUnitSyntax)base.VisitCompilationUnit(node)!;

        // Add using AutoCtor;
        if (!node.Usings.Any(u => u.Name is IdentifierNameSyntax { Identifier.Text: "AutoCtor" }
            || u.Name is AliasQualifiedNameSyntax { Name.Identifier.Text: "AutoCtor" }))
        {
            var usingName = IdentifierName("AutoCtor");
            var usingDirective = UsingDirective(usingName);
            node = node.AddUsings(usingDirective);
        }

        return node;
    }

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var isTargetOrAncestor = _targetAndAncestors.Any(a => a.IsEquivalentTo(node));

        // Remove matching constructor
        // Add [AutoConstruct] attribute
        if (node.IsEquivalentTo(typeDeclaration))
        {
            node = RemoveCtor(node);
            if (!HasAutoConstructAttribute(node))
            {
                var attrList = CreateAutoConstructAttributeList();
                node = node.AddAttributeLists(attrList);
            }
        }

        // Add partial modifier only to the target type and its containing types
        if (isTargetOrAncestor && !node.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            var partialToken = CreatePartialToken(node.Modifiers.Count == 0);
            node = node.AddModifiers(partialToken);
        }

        return base.VisitClassDeclaration(node);
    }

    public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
    {
        var isTargetOrAncestor = _targetAndAncestors.Any(a => a.IsEquivalentTo(node));

        // Remove matching constructor
        // Add [AutoConstruct] attribute
        if (node.IsEquivalentTo(typeDeclaration))
        {
            node = RemoveCtor(node);
            if (!HasAutoConstructAttribute(node))
            {
                var attrList = CreateAutoConstructAttributeList();
                node = node.AddAttributeLists(attrList);
            }
        }

        // Add partial modifier only to the target type and its containing types
        if (isTargetOrAncestor && !node.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            var partialToken = CreatePartialToken(node.Modifiers.Count == 0);
            node = node.AddModifiers(partialToken);
        }

        return base.VisitStructDeclaration(node);
    }

    public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        var isTargetOrAncestor = _targetAndAncestors.Any(a => a.IsEquivalentTo(node));

        // Remove matching constructor
        // Add [AutoConstruct] attribute
        if (node.IsEquivalentTo(typeDeclaration))
        {
            node = RemoveCtor(node);
            if (!HasAutoConstructAttribute(node))
            {
                var attrList = CreateAutoConstructAttributeList();
                node = node.AddAttributeLists(attrList);
            }
        }

        // Add partial modifier only to the target type and its containing types
        if (isTargetOrAncestor && !node.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            var partialToken = CreatePartialToken(node.Modifiers.Count == 0);
            node = node.AddModifiers(partialToken);
        }

        return base.VisitRecordDeclaration(node);
    }

    private TRoot RemoveCtor<TRoot>(TRoot node) where TRoot : SyntaxNode
    {
        if (ctorDeclaration is null)
            return node;
        var ctor = node.ChildNodes().First(ctor => ctor.IsEquivalentTo(ctorDeclaration));
        return node.RemoveNode(ctor, SyntaxRemoveOptions.KeepUnbalancedDirectives)!;
    }

    private static SyntaxToken CreatePartialToken(bool noSpace)
    {
        return Token(
            SyntaxTriviaList.Empty,
            SyntaxKind.PartialKeyword,
            noSpace ? SyntaxTriviaList.Empty : SyntaxTriviaList.Create(Space));
    }

    private static AttributeListSyntax CreateAutoConstructAttributeList()
    {
        var attrName = IdentifierName("AutoConstruct");
        var attr = Attribute(attrName);
        return AttributeList(SingletonSeparatedList(attr))
            .WithTrailingTrivia(ElasticEndOfLine("\n"));
    }

    private static bool HasAutoConstructAttribute(TypeDeclarationSyntax typeDeclaration)
    {
        foreach (var attributeList in typeDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var simplifiedName = attribute.Name switch
                {
                    IdentifierNameSyntax i => i,
                    QualifiedNameSyntax q => q.Right,
                    AliasQualifiedNameSyntax a => a.Name,
                    _ => null
                };

                if (simplifiedName?.Identifier.Text == "AutoConstruct")
                    return true;
            }
        }

        return false;
    }
}

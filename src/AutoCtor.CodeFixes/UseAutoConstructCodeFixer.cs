using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoCtor.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class UseAutoConstructCodeFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Diagnostics.ACTR007_UseAutoConstruct.Id);

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root is null)
            return;

        var token = root.FindToken(context.Diagnostics[0].Location.SourceSpan.Start);
        var ctorDeclaration = token.Parent?.AncestorsAndSelf()
            .OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault();
        if (ctorDeclaration is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use [AutoConstruct]",
                createChangedDocument: ct => ApplyFixAsync(context.Document, ctorDeclaration, ct),
                equivalenceKey: "UseAutoConstruct"),
            context.Diagnostics[0]);
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        ConstructorDeclarationSyntax ctorDeclaration,
        CancellationToken cancellationToken)
    {
        var root = (CompilationUnitSyntax?)await document.GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);

        if (root is null)
            return document;

        root = (CompilationUnitSyntax)new AutoCtorRewriter(ctorDeclaration).Visit(root);
        return document.WithSyntaxRoot(root);
    }

    private sealed class AutoCtorRewriter(ConstructorDeclarationSyntax ctorDeclaration) : CSharpSyntaxRewriter
    {
        // The target type and all containing types – the only types that need 'partial'.
        private readonly ImmutableArray<BaseTypeDeclarationSyntax> _targetAndAncestors =
            ctorDeclaration
                .Ancestors()
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
            if (node.IsEquivalentTo(ctorDeclaration.Parent))
            {
                node = RemoveCtor(node);
                var attrList = CreateAutoConstructAttributeList();
                node = node.AddAttributeLists(attrList);
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
            if (node.IsEquivalentTo(ctorDeclaration.Parent))
            {
                node = RemoveCtor(node);
                var attrList = CreateAutoConstructAttributeList();
                node = node.AddAttributeLists(attrList);
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
            if (node.IsEquivalentTo(ctorDeclaration.Parent))
            {
                node = RemoveCtor(node);
                var attrList = CreateAutoConstructAttributeList();
                node = node.AddAttributeLists(attrList);
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
    }
}

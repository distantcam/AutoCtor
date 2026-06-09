using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        var root = await document.GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);

        if (root is null)
            return document;

        root = (CompilationUnitSyntax)new AutoCtorRewriter(
            (TypeDeclarationSyntax)ctorDeclaration.Parent!, ctorDeclaration)
            .Visit(root);
        return document.WithSyntaxRoot(root);
    }
}

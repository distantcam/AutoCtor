using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;

namespace AutoCtor.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public sealed class AddAutoConstructCodeFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Diagnostics.ACTR008_AddAutoConstruct.Id);

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root is null)
            return;

        var token = root.FindToken(context.Diagnostics[0].Location.SourceSpan.Start);
        if (token.Parent is not TypeDeclarationSyntax typeDeclaration)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add [AutoConstruct]",
                createChangedDocument: ct => ApplyFixAsync(context.Document, typeDeclaration, ct),
                equivalenceKey: "AddAutoConstruct"),
            context.Diagnostics[0]);
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        TypeDeclarationSyntax typeDeclaration,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);

        if (root is null)
            return document;

        root = (CompilationUnitSyntax)new AutoCtorRewriter(typeDeclaration, null)
            .Visit(root);
        return document.WithSyntaxRoot(root);
    }
}

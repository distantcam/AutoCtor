using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TUnit.Assertions.Core;

internal static class TestHelper
{
    public static CancellationToken CancellationToken =>
        TestContext.Current?.Execution?.CancellationToken ?? CancellationToken.None;

    public static ISourceGenerator AsSourceGenerator(this ISourceGenerator generator) => generator;

    public static ConfiguredTaskAwaitable<TValue?> ConfigureAwait<TValue>(this Assertion<TValue> assertion, bool continueOnCapturedContext)
    {
        return assertion.AssertAsync().ConfigureAwait(continueOnCapturedContext);
    }

    public static async Task ApplyCodeFix(
        Workspace workspace,
        DocumentId documentId,
        DiagnosticAnalyzer diagnosticAnalyzer,
        CodeFixProvider fixer,
        CancellationToken cancellation = default)
    {
        var diagnosticIds = diagnosticAnalyzer.SupportedDiagnostics.Select(x => x.Id).ToImmutableArray();

        while (true)
        {
            var currentDocument = workspace.CurrentSolution.GetDocument(documentId)!;
            var currentCompilation = await currentDocument.Project
                .GetCompilationAsync(cancellation)
                .ConfigureAwait(false);
            var withAnalyzers = currentCompilation!.WithAnalyzers([diagnosticAnalyzer]);
            var allDiagnostics = await withAnalyzers
                .GetAllDiagnosticsAsync(cancellation)
                .ConfigureAwait(false);
            var diagnostics = allDiagnostics.Where(d => diagnosticIds.Contains(d.Id)).ToImmutableArray();

            if (diagnostics.IsEmpty)
                break;

            var diagnostic = diagnostics[0];
            var actions = new List<CodeAction>();
            var fixContext = new CodeFixContext(
                currentDocument,
                diagnostic,
                (action, _) => actions.Add(action),
                cancellation);

            await fixer.RegisterCodeFixesAsync(fixContext)
                .ConfigureAwait(false);

            var changed = false;
            foreach (var action in actions)
            {
                var operations = await action.GetOperationsAsync(cancellation)
                    .ConfigureAwait(false);
                foreach (var applyOp in operations.OfType<ApplyChangesOperation>())
                {
                    applyOp.Apply(workspace, cancellation);
                    changed = true;
                }
            }

            if (!changed)
                break;
        }
    }
}

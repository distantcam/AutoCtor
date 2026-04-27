using System.Collections.Immutable;
using AutoCtor;
using AutoCtor.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using static ExampleTestsHelper;

internal sealed class CodeFixTests
{
    [Test]
    [CombinedDataSources]
    public async Task ApplyCodeFix(
        [MethodDataSource(nameof(GetExamples))] CodeFileTheoryData theoryData,
        [ClassDataSource<CompilationBuilderFactory<AutoConstructAttribute>>(Shared = SharedType.PerTestSession)]
        CompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Create(theoryData);
        var compilation = builder.Build(nameof(CodeFixTests));
        var fixer = new UseAutoConstructCodeFixer();

        using var workspace = new AdhocWorkspace();

        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var solution = workspace.CurrentSolution
            .AddProject(projectId, nameof(CodeFixTests), nameof(CodeFixTests), LanguageNames.CSharp);
        foreach (var reference in compilation.References)
            solution = solution.AddMetadataReference(projectId, reference);
        solution = solution.AddDocument(documentId, theoryData.ToString(),
            string.Join(Environment.NewLine, theoryData.Codes));
        workspace.TryApplyChanges(solution);

        while (true)
        {
            var currentDocument = workspace.CurrentSolution.GetDocument(documentId)!;
            var currentCompilation = await currentDocument.Project
                .GetCompilationAsync(TestHelper.CancellationToken)
                .ConfigureAwait(false);
            var withAnalyzers = currentCompilation!.WithAnalyzers([new UseAutoConstructAnalyzer()]);
            var allDiagnostics = await withAnalyzers
                .GetAllDiagnosticsAsync(TestHelper.CancellationToken)
                .ConfigureAwait(false);
            var diagnostics = allDiagnostics.Where(d => d.Id == "ACTR007").ToImmutableArray();

            if (diagnostics.IsEmpty)
                break;

            var diagnostic = diagnostics[0];
            var actions = new List<CodeAction>();
            var fixContext = new CodeFixContext(
                currentDocument,
                diagnostic,
                (action, _) => actions.Add(action),
                TestHelper.CancellationToken);

            await fixer.RegisterCodeFixesAsync(fixContext)
                .ConfigureAwait(false);

            var changed = false;
            foreach (var action in actions)
            {
                var operations = await action.GetOperationsAsync(TestHelper.CancellationToken)
                    .ConfigureAwait(false);
                foreach (var applyOp in operations.OfType<ApplyChangesOperation>())
                {
                    applyOp.Apply(workspace, TestHelper.CancellationToken);
                    changed = true;
                }
            }

            if (!changed)
                break;
        }

        var newDocument = workspace.CurrentSolution.GetDocument(documentId)!;
        var newText = await newDocument.GetTextAsync(TestHelper.CancellationToken)
            .ConfigureAwait(false);

        var verifyText = string.Join(Environment.NewLine,
            ["// Before: ", "", .. theoryData.Codes, "", "// After: ", "", newText]);

        await Verify(verifyText)
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name)
            .UseMethodName("cs")
            .IgnoreParameters()
            .ConfigureAwait(false);
    }

    public static IEnumerable<Func<CodeFileTheoryData>> GetExamples()
    {
        foreach (var codeFixExample in GetExamplesFiles("CodeFixExamples"))
        {
            yield return () => new CodeFileTheoryData(codeFixExample);
        }
    }
}

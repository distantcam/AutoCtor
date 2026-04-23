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
        [ClassDataSource<CompilationBuilderFactoryBase<AutoConstructAttribute>>(Shared = SharedType.PerTestSession)]
        CompilationBuilderFactoryBase builderFactory)
    {
        var builder = builderFactory.Create(theoryData);
        var compilation = builder.Build(nameof(CodeFixTests));

        var compilationWithAnalyzers = compilation.WithAnalyzers([new UseAutoConstructAnalyzer()]);

        var allDiagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync(TestHelper.CancellationToken)
            .ConfigureAwait(false);
        var diagnostics = allDiagnostics.Where(d => d.Id == "ACTR007").ToImmutableArray();

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

        var fixer = new UseAutoConstructCodeFixer();

        foreach (var diagnostic in diagnostics)
        {
            var currentDocument = workspace.CurrentSolution.GetDocument(documentId)!;
            var actions = new List<CodeAction>();
            var fixContext = new CodeFixContext(
                currentDocument,
                diagnostic,
                (action, _) => actions.Add(action),
                TestHelper.CancellationToken);

            await fixer.RegisterCodeFixesAsync(fixContext).ConfigureAwait(false);

            await Assert.That(actions.Count).IsGreaterThan(0);
            var operations = await actions[0].GetOperationsAsync(TestHelper.CancellationToken)
                .ConfigureAwait(false);
            var applyOp = operations.OfType<ApplyChangesOperation>().First();
            applyOp.Apply(workspace, TestHelper.CancellationToken);
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

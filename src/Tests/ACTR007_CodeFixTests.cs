using AutoCtor;
using AutoCtor.CodeFixes;
using Tests.Utilities;
using static ExampleTestsHelper;

internal sealed class ACTR007_CodeFixTests
{
    [Test]
    [CombinedDataSources]
    public async Task ApplyCodeFix(
        [MethodDataSource(nameof(GetExamples))] CodeFileTheoryData theoryData,
        [ClassDataSource<CompilationBuilderFactory<AutoConstructAttribute>>(Shared = SharedType.PerTestSession)]
        CompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Create(theoryData);
        var compilation = builder.Build(nameof(ACTR007_CodeFixTests));

        var workspaceBuilder = new WorkspaceBuilder()
            .WithName(nameof(ACTR007_CodeFixTests))
            .WithReferences(compilation.References);
        var documentId = workspaceBuilder.AddDocument(theoryData.ToString(), string.Join(Environment.NewLine, theoryData.Codes));

        using var workspace = workspaceBuilder.Build();

        await TestHelper.ApplyCodeFix(
            workspace,
            documentId,
            new UseAutoConstructAnalyzer(),
            new UseAutoConstructCodeFixer(),
            TestHelper.CancellationToken)
            .ConfigureAwait(false);

        var newDocument = workspace.CurrentSolution.GetDocument(documentId)!;
        var newText = await newDocument.GetTextAsync(TestHelper.CancellationToken)
            .ConfigureAwait(false);

        var verifyText = newText.ToString();

        await Verify(verifyText)
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name)
            .UseMethodName("cs")
            .IgnoreParameters()
            .ConfigureAwait(false);
    }

    public static IEnumerable<Func<CodeFileTheoryData>> GetExamples()
    {
        foreach (var codeFixExample in GetExamplesFiles("ACTR007_CodeFixExamples"))
        {
            yield return () => new CodeFileTheoryData(codeFixExample);
        }
    }
}

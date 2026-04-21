using AutoCtor;
using Microsoft.CodeAnalysis.Diagnostics;
using static ExampleTestsHelper;

internal sealed class CodeAnalyzerTests
{
    [Test]
    [CombinedDataSources]
    public async Task CheckDiagnostics(
        [MethodDataSource(nameof(GetExamples))] CodeFileTheoryData theoryData,
        [ClassDataSource<CompilationBuilderFactoryBase<AutoConstructAttribute>>(Shared = SharedType.PerTestSession)]
        CompilationBuilderFactoryBase builderFactory)
    {
        var builder = builderFactory.Create(theoryData);
        var compilation = builder.Build(nameof(CodeAnalyzerTests));

        var compilationWithAnalyzers = compilation.WithAnalyzers([new UseAutoConstructAnalyzer()]);

        var diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync(TestHelper.CancellationToken)
            .ConfigureAwait(false);

        await Verify(diagnostics.Where(d => d.Id == "ACTR007"))
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name)
            .UseMethodName("cs")
            .IgnoreParameters()
            .ConfigureAwait(false);
    }

    public static IEnumerable<Func<CodeFileTheoryData>> GetExamples()
    {
        foreach (var codeAnalyzerExample in GetExamplesFiles("CodeAnalyzerExamples"))
        {
            yield return () => new CodeFileTheoryData(codeAnalyzerExample);
        }
    }
}

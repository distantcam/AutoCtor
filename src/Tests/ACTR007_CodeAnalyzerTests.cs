using System.Collections.Immutable;
using AutoCtor;
using Microsoft.CodeAnalysis.Diagnostics;
using static ExampleTestsHelper;

internal sealed class ACTR007_CodeAnalyzerTests
{
    [Test]
    [CombinedDataSources]
    public async Task CheckDiagnostics(
        [MethodDataSource(nameof(GetExamples))] CodeFileTheoryData theoryData,
        [ClassDataSource<CompilationBuilderFactory<AutoConstructAttribute>>(Shared = SharedType.PerTestSession)]
        CompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Create(theoryData);
        var compilation = builder.Build(nameof(ACTR007_CodeAnalyzerTests));

        var compilationWithAnalyzers = compilation.WithAnalyzers([new UseAutoConstructAnalyzer()]);

        var diagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync(TestHelper.CancellationToken)
            .ConfigureAwait(false);

        await Verify(diagnostics
                .Where(d => !theoryData.IgnoredCompileDiagnostics.Contains(d.Id)))
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name)
            .UseMethodName("cs")
            .IgnoreParameters()
            .ConfigureAwait(false);
    }

    public static IEnumerable<Func<CodeFileTheoryData>> GetExamples()
    {
        foreach (var codeAnalyzerExample in GetExamplesFiles("ACTR007_CodeAnalyzerExamples"))
        {
            yield return () => new CodeFileTheoryData(codeAnalyzerExample) with
            {
                IgnoredCompileDiagnostics = [
                    "CS0414", "CS0169", // Ignore unused fields
                    "CS8618", // Non-null field must be set after exiting constructor
                    "CS8019" // Unnecessary using directive
                ]
            };
        }
    }
}

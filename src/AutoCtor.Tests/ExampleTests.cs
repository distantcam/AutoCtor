using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static ExampleTestsHelper;

namespace AutoCtor.Tests;

public class ExampleTests
{
    [Test]
    [MethodDataSource(nameof(GetExamples))]
    public async Task ExamplesGeneratedCode(CodeFileTheoryData theoryData)
    {
        var builder = CreateCompilation(theoryData);
        var compilation = await builder.Build(nameof(ExampleTests), TestHelper.CancellationToken)
            .ConfigureAwait(false);
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .WithAnalyzerOptions(theoryData.Options)
            .Build(builder.ParseOptions)
            .RunGenerators(compilation, TestHelper.CancellationToken);

        await Verify(driver)
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name)
            .IgnoreParametersForVerified()
            .ConfigureAwait(false);
    }

    [Test]
    [MethodDataSource(nameof(GetExamples))]
    public async Task CodeCompilesProperly(CodeFileTheoryData theoryData)
    {
        var builder = CreateCompilation(theoryData);
        var compilation = await builder.Build(nameof(ExampleTests), TestHelper.CancellationToken)
            .ConfigureAwait(false);
        new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .WithAnalyzerOptions(theoryData.Options)
            .Build(builder.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out _,
                TestHelper.CancellationToken);

        await Assert.That(outputCompilation.GetDiagnostics(TestHelper.CancellationToken)
            .Where(d => !theoryData.IgnoredCompileDiagnostics.Contains(d.Id)))
            .IsEmpty();
    }

#if ROSLYN_4_4
    [Test]
    [MethodDataSource(nameof(GetExamples))]
    public async Task EnsureRunsAreCachedCorrectly(CodeFileTheoryData theoryData)
    {
        var builder = CreateCompilation(theoryData);
        var compilation = await builder.Build(nameof(ExampleTests), TestHelper.CancellationToken)
            .ConfigureAwait(false);

        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .WithAnalyzerOptions(theoryData.Options)
            .Build(builder.ParseOptions);

        driver = driver.RunGenerators(compilation, TestHelper.CancellationToken);
        var firstResult = driver.GetRunResult();

        // Change the compilation
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("// dummy",
            CSharpParseOptions.Default.WithLanguageVersion(theoryData.LangPreview
                ? LanguageVersion.Preview
                : LanguageVersion.Latest),
            cancellationToken: TestHelper.CancellationToken));

        driver = driver.RunGenerators(compilation, TestHelper.CancellationToken);
        var secondResult = driver.GetRunResult();

        await AssertRunsEqual(firstResult, secondResult,
            AutoConstructSourceGenerator.TrackingNames.AllTrackers)
            .ConfigureAwait(false);
    }
#endif

    // ----------------------------------------------------------------------------------------

    private static CompilationBuilder CreateCompilation(CodeFileTheoryData theoryData)
    {
        var version = TestFileHelper.GetPackageVersion("Microsoft.Extensions.DependencyInjection.Abstractions");

        return CreateCompilation<AutoConstructAttribute>(theoryData)
            .AddNugetReference("Microsoft.Extensions.DependencyInjection.Abstractions", version);
    }

    private static IEnumerable<string> GetExamplesFiles(string path)
        => Directory.GetFiles(Path.Combine(TestFileHelper.BaseDir?.FullName ?? "", path), "*.cs")
        .Where(e => !e.Contains(".g."));

    public static IEnumerable<Func<CodeFileTheoryData>> GetExamples()
    {
        if (TestFileHelper.BaseDir is null)
            throw new Exception("BaseDir is null");

        foreach (var example in GetExamplesFiles("Examples"))
        {
            yield return () => new CodeFileTheoryData(example) with
            {
                IgnoredCompileDiagnostics = ["CS0414", "CS0169"] // Ignore unused fields
            };
        }

        foreach (var guardExample in GetExamplesFiles("GuardExamples"))
        {
            yield return () => new CodeFileTheoryData(guardExample) with
            {
                Options = new() { { "build_property.AutoCtorGuards", "true" } }
            };
        }

        foreach (var langExample in GetExamplesFiles("LangExamples"))
        {
            var verifiedName = string.Concat("Verified_", PreprocessorSymbols.Last().AsSpan(7));
            yield return () => new CodeFileTheoryData(langExample) with
            {
                VerifiedDirectory = Path.Combine(Path.GetDirectoryName(langExample) ?? "", verifiedName),
                LangPreview = true,
            };
        }
    }
}

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

#if ROSLYN_4_4
using Microsoft.CodeAnalysis.CSharp;
#endif

namespace AutoCtor.Tests;

public class ExampleTests
{
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task ExamplesGeneratedCode(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile<AutoConstructAttribute>(theoryData.Codes,
            langPreview: theoryData.LangPreview,
            preprocessorSymbols: s_preprocessorSymbols);
        var generator = new AutoConstructSourceGenerator().AsSourceGenerator();
        var driver = Helpers.CreateDriver(theoryData.Options, theoryData.LangPreview, generator)
            .RunGenerators(compilation);

        await Verify(driver)
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name).IgnoreParametersForVerified(theoryData);
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task CodeCompilesProperly(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile<AutoConstructAttribute>(theoryData.Codes,
            langPreview: theoryData.LangPreview,
            preprocessorSymbols: s_preprocessorSymbols);
        var generator = new AutoConstructSourceGenerator().AsSourceGenerator();
        Helpers.CreateDriver(theoryData.Options, theoryData.LangPreview, generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        Assert.Empty(outputCompilation.GetDiagnostics().Where(d => !theoryData.IgnoredCompileDiagnostics.Contains(d.Id)));
    }

#if ROSLYN_4_4
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task EnsureRunsAreCachedCorrectly(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile<AutoConstructAttribute>(theoryData.Codes,
            langPreview: theoryData.LangPreview,
            preprocessorSymbols: s_preprocessorSymbols);
        var generator = new AutoConstructSourceGenerator().AsSourceGenerator();

        var driver = Helpers.CreateDriver(theoryData.Options, theoryData.LangPreview, generator);
        driver = driver.RunGenerators(compilation);
        var firstResult = driver.GetRunResult();
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("// dummy",
            CSharpParseOptions.Default.WithLanguageVersion(theoryData.LangPreview
                ? LanguageVersion.Preview
                : LanguageVersion.Latest)));
        driver = driver.RunGenerators(compilation);
        var secondResult = driver.GetRunResult();

        Helpers.AssertRunsEqual(firstResult, secondResult,
            AutoConstructSourceGenerator.TrackingNames.AllTrackers);
    }
#endif

    // ----------------------------------------------------------------------------------------

    private static readonly IEnumerable<string> s_preprocessorSymbols = [
#if ROSLYN_3
        "ROSLYN_3",
#endif
#if ROSLYN_3_11
        "ROSLYN_3_11",
#endif
#if ROSLYN_4
        "ROSLYN_4",
#endif
#if ROSLYN_4_0
        "ROSLYN_4_0",
#endif
#if ROSLYN_4_4
        "ROSLYN_4_4",
#endif
    ];

    private static DirectoryInfo? BaseDir { get; } = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;

    private static IEnumerable<string> GetExamplesFiles(string path) => Directory.GetFiles(Path.Combine(BaseDir?.FullName ?? "", path), "*.cs").Where(e => !e.Contains(".g."));

    public static TheoryData<CodeFileTheoryData> GetExamples()
    {
        if (BaseDir == null)
            throw new Exception("BaseDir is null");

        var data = new TheoryData<CodeFileTheoryData>();

        foreach (var example in GetExamplesFiles("Examples"))
        {
            data.Add(new CodeFileTheoryData(example) with
            {
                IgnoredCompileDiagnostics = ["CS0414", "CS0169"] // Ignore unused fields
            });
        }

        foreach (var guardExample in GetExamplesFiles("GuardExamples"))
        {
            data.Add(new CodeFileTheoryData(guardExample) with
            {
                Options = [new("build_property.AutoCtorGuards", "true")]
            });
        }

        foreach (var langExample in GetExamplesFiles("LangExamples"))
        {
            var verifiedName = "Verified_" + s_preprocessorSymbols.Last().Substring(7);
            data.Add(new CodeFileTheoryData(langExample) with
            {
                VerifiedDirectory = Path.Combine(Path.GetDirectoryName(langExample) ?? "", verifiedName),
                LangPreview = true
            });
        }

        return data;
    }
}

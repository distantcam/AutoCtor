using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

namespace AutoCtor.Tests;

public class ExampleTests
{
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task ExamplesGeneratedCode(CodeFileTheoryData theoryData)
    {

        var compilation = await Helpers.Compile<AutoConstructAttribute>(theoryData.Codes,
            preprocessorSymbols: PreprocessorSymbols);
        var generator = new AutoConstructSourceGenerator().AsSourceGenerator();
        var driver = Helpers.CreateDriver(theoryData.Options, generator)
            .RunGenerators(compilation);

        await Verify(driver)
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name);
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task CodeCompilesProperly(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile<AutoConstructAttribute>(theoryData.Codes,
            preprocessorSymbols: PreprocessorSymbols);
        var generator = new AutoConstructSourceGenerator().AsSourceGenerator();
        Helpers.CreateDriver(theoryData.Options, generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        outputCompilation.GetDiagnostics()
            .Where(d => !theoryData.IgnoredCompileDiagnostics.Contains(d.Id))
            .Should().BeEmpty();
    }

#if ROSLYN_4
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task EnsureRunsAreCachedCorrectly(CodeFileTheoryData theoryData)
    {
        var compilation = await Helpers.Compile<AutoConstructAttribute>(theoryData.Codes,
            preprocessorSymbols: PreprocessorSymbols);
        var generator = new AutoConstructSourceGenerator().AsSourceGenerator();

        var driver = Helpers.CreateDriver(theoryData.Options, generator);
        driver = driver.RunGenerators(compilation);
        var firstResult = driver.GetRunResult();
        compilation = compilation.AddSyntaxTrees(
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText("// dummy"));
        driver = driver.RunGenerators(compilation);
        var secondResult = driver.GetRunResult();

        Helpers.AssertRunsEqual(firstResult, secondResult,
            AutoConstructSourceGenerator.TrackingNames.AllTrackers);
    }
#endif

    // ----------------------------------------------------------------------------------------

    private static IEnumerable<string> PreprocessorSymbols =
#if ROSLYN_3
        ["ROSLYN_3"];
#elif ROSLYN_4
        ["ROSLYN_4"];
#endif

    private static DirectoryInfo? BaseDir { get; } = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;

    private static IEnumerable<string> GetExamplesFiles(string path) => Directory.GetFiles(Path.Combine(BaseDir?.FullName ?? "", path), "*.cs").Where(e => !e.Contains(".g."));

    public static TheoryData<CodeFileTheoryData> GetExamples()
    {
        if (BaseDir == null)
            throw new Exception("BaseDir is null");

        var data = new TheoryData<CodeFileTheoryData>();

        var exampleCode = File.ReadAllText(Path.Combine(BaseDir.FullName, "IExampleInterfaces.cs"));

        foreach (var example in GetExamplesFiles("Examples"))
        {
            data.Add(new CodeFileTheoryData(example, exampleCode) with
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
#if ROSLYN_3_11
            var verifiedName = "Verified_3_11";
#elif ROSLYN_4_4
            var verifiedName = "Verified_4_4";
#elif ROSLYN_4_6
            var verifiedName = "Verified_4_6";
#elif ROSLYN_4_8
            var verifiedName = "Verified_4_8";
#elif ROSLYN_4_10
            var verifiedName = "Verified_4_10";
#endif
            data.Add(new CodeFileTheoryData(langExample) with
            {
                VerifiedDirectory = Path.Combine(Path.GetDirectoryName(langExample) ?? "", verifiedName)
            });
        }

        return data;
    }
}

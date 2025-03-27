﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static ExampleTestsHelper;

namespace AutoCtor.Tests;

public class ExampleTests
{
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task ExamplesGeneratedCode(CodeFileTheoryData theoryData)
    {
        var builder = CreateCompilation<AutoConstructAttribute>(theoryData);
        var compilation = await builder.Build(nameof(ExampleTests));
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .WithAnalyzerOptions(theoryData.Options)
            .Build(builder.ParseOptions)
            .RunGenerators(compilation);

        await Verify(driver)
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name).IgnoreParametersForVerified(theoryData);
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task CodeCompilesProperly(CodeFileTheoryData theoryData)
    {
        var builder = CreateCompilation<AutoConstructAttribute>(theoryData);
        var compilation = await builder.Build(nameof(ExampleTests));
        new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .WithAnalyzerOptions(theoryData.Options)
            .Build(builder.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        Assert.Empty(outputCompilation.GetDiagnostics().Where(d => !theoryData.IgnoredCompileDiagnostics.Contains(d.Id)));
    }

#if ROSLYN_4_4
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task EnsureRunsAreCachedCorrectly(CodeFileTheoryData theoryData)
    {
        var builder = CreateCompilation<AutoConstructAttribute>(theoryData);
        var compilation = await builder.Build(nameof(ExampleTests));

        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .WithAnalyzerOptions(theoryData.Options)
            .Build(builder.ParseOptions);

        driver = driver.RunGenerators(compilation);
        var firstResult = driver.GetRunResult();

        // Change the compilation
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("// dummy",
            CSharpParseOptions.Default.WithLanguageVersion(theoryData.LangPreview
                ? LanguageVersion.Preview
                : LanguageVersion.Latest)));

        driver = driver.RunGenerators(compilation);
        var secondResult = driver.GetRunResult();

        AssertRunsEqual(firstResult, secondResult,
            AutoConstructSourceGenerator.TrackingNames.AllTrackers);
    }
#endif

    // ----------------------------------------------------------------------------------------

    private static DirectoryInfo? BaseDir { get; } = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent;

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
                Options = new() { { "build_property.AutoCtorGuards", "true" } }
            });
        }

        foreach (var langExample in GetExamplesFiles("LangExamples"))
        {
            var verifiedName = string.Concat("Verified_", PreprocessorSymbols.Last().AsSpan(7));
            data.Add(new CodeFileTheoryData(langExample) with
            {
                VerifiedDirectory = Path.Combine(Path.GetDirectoryName(langExample) ?? "", verifiedName),
                LangPreview = true
            });
        }

        return data;
    }
}

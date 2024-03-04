using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Xunit.Abstractions;

namespace AutoCtor.Tests;

public class ExampleTests
{
    private readonly VerifySettings _codeVerifySettings;

    public ExampleTests()
    {
        _codeVerifySettings = new();
        _codeVerifySettings.ScrubLinesContaining("Version:", "SHA:", "GeneratedCodeAttribute");
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task ExamplesGeneratedCode(CodeFileTheoryData theoryData)
    {
        var exampleInterfaces = File.ReadAllText(Path.Combine(BaseDir.FullName, "Examples", "IExampleInterfaces.cs"));

        var compilation = await Compile(theoryData.Code, exampleInterfaces);
        var generator = new AutoConstructSourceGenerator();
        var driver = CreateDriver(compilation, [("build_property.AutoCtorGuards", theoryData.Guard.ToString())], generator).RunGenerators(compilation);

        await Verify(driver, _codeVerifySettings)
            .UseDirectory(Path.Combine(theoryData.Guard ? "GuardExamples" : "Examples", "Verified"))
            .UseTypeName(theoryData.Name);
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task CodeCompilesProperly(CodeFileTheoryData theoryData)
    {
        string[] ignoredWarnings = ["CS0414"]; // Ignore unused fields

        var exampleInterfaces = File.ReadAllText(Path.Combine(BaseDir.FullName, "Examples", "IExampleInterfaces.cs"));

        var compilation = await Compile(theoryData.Code, exampleInterfaces);
        var generator = new AutoConstructSourceGenerator();
        CreateDriver(compilation, [("build_property.AutoCtorGuards", theoryData.Guard.ToString())], generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        outputCompilation.GetDiagnostics()
            .Where(d => !ignoredWarnings.Contains(d.Id))
            .Should().BeEmpty();
    }

#if ROSLYN_3_11
    private static GeneratorDriver CreateDriver(
        Compilation c,
        IEnumerable<(string, string)> options,
        params ISourceGenerator[] generators)
        => CSharpGeneratorDriver.Create(generators,
            parseOptions: c.SyntaxTrees.FirstOrDefault().Options as CSharpParseOptions,
            optionsProvider: new TestAnalyzerConfigOptionsProvider(options));
#elif ROSLYN_4_0 || ROSLYN_4_4
    private static GeneratorDriver CreateDriver(
        Compilation c,
        IEnumerable<(string, string)> options,
        params IIncrementalGenerator[] generators)
        => CSharpGeneratorDriver.Create(generators)
        .WithUpdatedParseOptions(c.SyntaxTrees.FirstOrDefault().Options as CSharpParseOptions)
        .WithUpdatedAnalyzerConfigOptions(new TestAnalyzerConfigOptionsProvider(options));
#endif

    private static async Task<CSharpCompilation> Compile(params string[] code)
    {
        var references = await new ReferenceAssemblies(
            "net8.0",
            new PackageIdentity(
                "Microsoft.NETCore.App.Ref",
                "8.0.0"),
            Path.Combine("ref", "net8.0"))
            .ResolveAsync(null, CancellationToken.None);

        var attributeReference = MetadataReference.CreateFromFile(Path.Combine(Environment.CurrentDirectory, "AutoCtor.Attributes.dll"));

#if ROSLYN_3_11
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
#elif ROSLYN_4_0 || ROSLYN_4_4
        var options = CSharpParseOptions.Default;
#endif

        return CSharpCompilation.Create(
            "AutoCtorTest",
            code.Select(c => CSharpSyntaxTree.ParseText(c, options)),
            [attributeReference, .. references],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary
            ));
    }

    private static DirectoryInfo BaseDir { get; } = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;

    public static IEnumerable<object[]> GetExamples()
    {
        if (BaseDir == null)
            yield break;

        var examples = Directory.GetFiles(Path.Combine(BaseDir.FullName, "Examples"), "*.cs");
        foreach (var example in examples)
        {
            if (example.Contains(".g.") || example.Contains("IExampleInterfaces"))
                continue;

            yield return new object[] {
                new CodeFileTheoryData {
                    Code = File.ReadAllText(example),
                    Name = Path.GetFileNameWithoutExtension(example)
                }
            };
        }

        var guardExamples = Directory.GetFiles(Path.Combine(BaseDir.FullName, "GuardExamples"), "*.cs");
        foreach (var guardExample in guardExamples)
        {
            if (guardExample.Contains(".g.") || guardExample.Contains("IExampleInterfaces"))
                continue;

            yield return new object[] {
                new CodeFileTheoryData {
                    Code = File.ReadAllText(guardExample),
                    Name = Path.GetFileNameWithoutExtension(guardExample),
                    Guard = true
                }
            };
        }
    }

    public class CodeFileTheoryData : IXunitSerializable
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Guard { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>("Name");
            Code = info.GetValue<string>("Code");
            Guard = info.GetValue<bool>("Guard");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("Name", Name);
            info.AddValue("Code", Code);
            info.AddValue("Guard", Guard);
        }

        public override string ToString() => Name + ".cs";
    }

    private class TestAnalyzerConfigOptionsProvider(IEnumerable<(string, string)> options) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = new TestAnalyzerConfigOptions(options);
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;
    }

    private class TestAnalyzerConfigOptions(IEnumerable<(string key, string value)> options) : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _options = options.ToDictionary(e => e.key, e => e.value);

        public override bool TryGetValue(string key, [NotNullWhen(true)] out string value) =>
            _options.TryGetValue(key, out value);
    }
}

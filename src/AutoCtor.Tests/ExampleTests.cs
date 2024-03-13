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
        var compilation = await Compile(theoryData.Codes);
        var generator = new AutoConstructSourceGenerator();
        var driver = CreateDriver(compilation, theoryData.Options, generator)
            .RunGenerators(compilation);

        await Verify(driver, _codeVerifySettings)
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name);
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task CodeCompilesProperly(CodeFileTheoryData theoryData)
    {
        string[] ignoredWarnings = ["CS0414"]; // Ignore unused fields

        var compilation = await Compile(theoryData.Codes);
        var generator = new AutoConstructSourceGenerator();
        CreateDriver(compilation, theoryData.Options, generator)
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

    private static async Task<CSharpCompilation> Compile(IEnumerable<string> codes)
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
            codes.Select(c => CSharpSyntaxTree.ParseText(c, options)),
            [attributeReference, .. references],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary
            ));
    }

    private static DirectoryInfo BaseDir { get; } = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;

    public static TheoryData<CodeFileTheoryData> GetExamples()
    {
        if (BaseDir == null)
            throw new Exception("BaseDir is null");

        var data = new TheoryData<CodeFileTheoryData>();

        var examples = Directory.GetFiles(Path.Combine(BaseDir.FullName, "Examples"), "*.cs");
        var exampleCode = File.ReadAllText(Path.Combine(BaseDir.FullName, "Examples", "IExampleInterfaces.cs"));
        foreach (var example in examples)
        {
            if (example.Contains(".g.") || example.Contains("IExampleInterfaces"))
                continue;

            data.Add(
                new CodeFileTheoryData
                {
                    Name = Path.GetFileNameWithoutExtension(example),
                    Codes = [exampleCode, File.ReadAllText(example)],
                    Options = [],
                    VerifiedDirectory = Path.Combine(Path.GetDirectoryName(example), "Verified")
                }
            );
        }

        var guardExamples = Directory.GetFiles(Path.Combine(BaseDir.FullName, "GuardExamples"), "*.cs");
        foreach (var guardExample in guardExamples)
        {
            if (guardExample.Contains(".g."))
                continue;

            data.Add(
                new CodeFileTheoryData
                {
                    Name = Path.GetFileNameWithoutExtension(guardExample),
                    Codes = [File.ReadAllText(guardExample)],
                    Options = [new("build_property.AutoCtorGuards", "true")],
                    VerifiedDirectory = Path.Combine(Path.GetDirectoryName(guardExample), "Verified"),
                }
            );
        }

        return data;
    }

    public class CodeFileTheoryData : IXunitSerializable
    {
        public string Name { get; set; }
        public string[] Codes { get; set; }
        public (string, string)[] Options { get; set; }
        public string VerifiedDirectory { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>(nameof(Name));
            Codes = info.GetValue<string[]>(nameof(Codes));
            Options = info.GetValue<string[]>(nameof(Options))
                .Select(o => o.Split('|'))
                .Select(o => (o[0], o[1]))
                .ToArray();
            VerifiedDirectory = info.GetValue<string>(nameof(VerifiedDirectory));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(Codes), Codes);
            info.AddValue(nameof(Options), Options.Select(o => $"{o.Item1}|{o.Item2}").ToArray());
            info.AddValue(nameof(VerifiedDirectory), VerifiedDirectory);
        }

        public override string ToString() => Name + ".cs";
    }

    private class TestAnalyzerConfigOptionsProvider(IEnumerable<(string, string)> options) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = new TestAnalyzerConfigOptions(options);
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;
    }

    private class TestAnalyzerConfigOptions(IEnumerable<(string Key, string Value)> options) : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _options
            = options.ToDictionary(e => e.Key, e => e.Value);
        public override bool TryGetValue(string key, [NotNullWhen(true)] out string value) =>
            _options.TryGetValue(key, out value);
    }
}

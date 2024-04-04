using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace AutoCtor.Tests;

internal class Helpers
{
#if ROSLYN_3_11
    public static GeneratorDriver CreateDriver(
        IEnumerable<(string, string)> options,
        params ISourceGenerator[] generators)
        => CSharpGeneratorDriver.Create(generators,
            parseOptions: CreateParseOptions([]),
            optionsProvider: new TestAnalyzerConfigOptionsProvider(options));
#elif ROSLYN_4_0 || ROSLYN_4_4
    public static GeneratorDriver CreateDriver(
        IEnumerable<(string, string)> options,
        params IIncrementalGenerator[] generators)
        => CSharpGeneratorDriver.Create(generators)
            .WithUpdatedParseOptions(CreateParseOptions([]))
            .WithUpdatedAnalyzerConfigOptions(new TestAnalyzerConfigOptionsProvider(options));
#endif

    public static async Task<CSharpCompilation> Compile(
        IEnumerable<string> codes,
        string assemblyName = "AutoCtorTest",
        IEnumerable<string> preprocessorSymbols = default,
        IEnumerable<MetadataReference> extraReferences = default)
    {
        preprocessorSymbols ??= [];
        extraReferences ??= [];

        var references = await new ReferenceAssemblies(
            "net8.0",
            new PackageIdentity(
                "Microsoft.NETCore.App.Ref",
                "8.0.0"),
            Path.Combine("ref", "net8.0"))
            .ResolveAsync(null, CancellationToken.None);

        var attributeReference = MetadataReference.CreateFromFile(Path.Combine(Environment.CurrentDirectory, "AutoCtor.Attributes.dll"));

        var options = CreateParseOptions(preprocessorSymbols);

        return CSharpCompilation.Create(
            assemblyName,
            codes.Select(c => CSharpSyntaxTree.ParseText(c, options)),
            [attributeReference, .. references, .. extraReferences],
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary
            ));
    }

    private static CSharpParseOptions CreateParseOptions(IEnumerable<string> preprocessorSymbols)
    {
#if ROSLYN_3_11
        return CSharpParseOptions.Default
            .WithPreprocessorSymbols(ImmutableArray.Create(["ROSLYN_3_11", .. preprocessorSymbols]))
            .WithLanguageVersion(LanguageVersion.Preview);
#elif ROSLYN_4_0
        return CSharpParseOptions.Default
            .WithPreprocessorSymbols(ImmutableArray.Create(["ROSLYN_4_0", .. preprocessorSymbols]));
#elif ROSLYN_4_4
        return CSharpParseOptions.Default
            .WithPreprocessorSymbols(ImmutableArray.Create(["ROSLYN_4_4", .. preprocessorSymbols]));
#endif
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

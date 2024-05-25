using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace AutoCtor.Tests;

internal static class Helpers
{
    public static GeneratorDriver CreateDriver(
        IEnumerable<(string, string)> options,
        params ISourceGenerator[] generators)
    => CSharpGeneratorDriver.Create(
        generators: generators,
        parseOptions: CreateParseOptions([]),
        optionsProvider: new TestAnalyzerConfigOptionsProvider(options),
        driverOptions: new GeneratorDriverOptions(
            disabledOutputs: IncrementalGeneratorOutputKind.None,
            trackIncrementalGeneratorSteps: true));

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

    public static void AssertRunsEqual(
        GeneratorDriverRunResult runResult1,
        GeneratorDriverRunResult runResult2,
        IEnumerable<string> trackingNames)
    {
        using var _ = new AssertionScope();

        // We're given all the tracking names, but not all the
        // stages will necessarily execute, so extract all the
        // output steps, and filter to ones we know about
        var trackedSteps1 = GetTrackedSteps(runResult1, trackingNames);
        var trackedSteps2 = GetTrackedSteps(runResult2, trackingNames);

        // Both runs should have the same tracked steps
        trackedSteps1.Should()
                     .NotBeEmpty()
                     .And.HaveSameCount(trackedSteps2)
                     .And.ContainKeys(trackedSteps2.Keys);

        // Get the IncrementalGeneratorRunStep collection for each run
        foreach (var (trackingName, runSteps1) in trackedSteps1)
        {
            // Assert that both runs produced the same outputs
            var runSteps2 = trackedSteps2[trackingName];
            AssertStepsEqual(runSteps1, runSteps2, trackingName);
        }

        return;

        // Local function that extracts the tracked steps
        static Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> GetTrackedSteps(
            GeneratorDriverRunResult runResult, IEnumerable<string> trackingNames)
            => runResult
                    .Results[0] // We're only running a single generator, so this is safe
                    .TrackedSteps // Get the pipeline outputs
                    .Where(step => trackingNames.Contains(step.Key)) // filter to known steps
                    .ToDictionary(x => x.Key, x => x.Value); // Convert to a dictionary
    }

    private static void AssertStepsEqual(
        ImmutableArray<IncrementalGeneratorRunStep> runSteps1,
        ImmutableArray<IncrementalGeneratorRunStep> runSteps2,
        string stepName)
    {
        runSteps1.Should().HaveSameCount(runSteps2);

        for (var i = 0; i < runSteps1.Length; i++)
        {
            var runStep1 = runSteps1[i];
            var runStep2 = runSteps2[i];

            // The outputs should be equal between different runs
            var outputs1 = runStep1.Outputs.Select(x => x.Value);
            var outputs2 = runStep2.Outputs.Select(x => x.Value);

            outputs1.Should()
                    .Equal(outputs2, $"because {stepName} should produce cacheable outputs");

            // Therefore, on the second run the results should always be cached or unchanged!
            // - Unchanged is when the _input_ has changed, but the output hasn't
            // - Cached is when the the input has not changed, so the cached output is used
            runStep2.Outputs.Should()
                .AllSatisfy(x => x.Reason.Should()
                .BeOneOf([IncrementalStepRunReason.Cached, IncrementalStepRunReason.Unchanged],
                $"{stepName} expected to have reason {IncrementalStepRunReason.Cached} or {IncrementalStepRunReason.Unchanged}"));
        }
    }
}

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

internal static class Helpers
{
#if ROSLYN_3
    public static GeneratorDriver CreateDriver(
        IEnumerable<(string, string)> options,
        bool langPreview,
        params ISourceGenerator[] generators)
    => CSharpGeneratorDriver.Create(generators,
        parseOptions: CreateParseOptions(langPreview, []),
        optionsProvider: new TestAnalyzerConfigOptionsProvider(options));
#elif ROSLYN_4
    public static GeneratorDriver CreateDriver(
        IEnumerable<(string, string)> options,
        bool langPreview,
        params ISourceGenerator[] generators)
    => CSharpGeneratorDriver.Create(generators,
        parseOptions: CreateParseOptions(langPreview, []),
        optionsProvider: new TestAnalyzerConfigOptionsProvider(options),
        driverOptions: new GeneratorDriverOptions(
            disabledOutputs: IncrementalGeneratorOutputKind.None,
            trackIncrementalGeneratorSteps: true
        ));
#endif

    public static GeneratorDriver CreateDriver(params ISourceGenerator[] generators)
    => CreateDriver([], false, generators);

    public static async Task<CSharpCompilation> Compile<TAttribute>(
        IEnumerable<string> codes,
        string targetFramework = "net9.0",
        string netCoreVersion = "9.0.0",
        string assemblyName = "RoslynTests",
        bool langPreview = false,
        IEnumerable<string>? preprocessorSymbols = default,
        IEnumerable<MetadataReference>? extraReferences = default)
    {
        extraReferences ??= [];
        var attributeReference = MetadataReference.CreateFromFile(typeof(TAttribute).Assembly.Location);
        var netCoreRef = await GetNetCoreRef(targetFramework, netCoreVersion);
        return Compile(codes, assemblyName, langPreview, preprocessorSymbols ?? [], [attributeReference, .. netCoreRef, .. extraReferences]);
    }

    public static async Task<CSharpCompilation> Compile(
        IEnumerable<string> codes,
        string targetFramework = "net9.0",
        string netCoreVersion = "9.0.0",
        string assemblyName = "RoslynTests",
        bool langPreview = false,
        IEnumerable<string>? preprocessorSymbols = default,
        IEnumerable<MetadataReference>? extraReferences = default)
    {
        extraReferences ??= [];
        var netCoreRef = await GetNetCoreRef(targetFramework, netCoreVersion);
        return Compile(codes, assemblyName, langPreview, preprocessorSymbols ?? [], [.. netCoreRef, .. extraReferences]);
    }

    public static CSharpCompilation Compile(
        IEnumerable<string> codes,
        string assemblyName,
        bool langPreview,
        IEnumerable<string>? preprocessorSymbols,
        IEnumerable<MetadataReference>? references)
    {
        preprocessorSymbols ??= [];
        references ??= [];
        var options = CreateParseOptions(langPreview, preprocessorSymbols);
        return CSharpCompilation.Create(
            assemblyName,
            codes.Select(c => CSharpSyntaxTree.ParseText(c, options)),
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary
            ));
    }

    private static async Task<ImmutableArray<MetadataReference>> GetNetCoreRef(
        string targetFramework,
        string netCoreVersion)
    {
        return await new ReferenceAssemblies(
            targetFramework,
            new("Microsoft.NETCore.App.Ref", netCoreVersion),
            Path.Combine("ref", targetFramework))
            .ResolveAsync(null, CancellationToken.None);
    }

    private static CSharpParseOptions CreateParseOptions(bool langPreview, IEnumerable<string> preprocessorSymbols)
    {
        var options = CSharpParseOptions.Default
            .WithPreprocessorSymbols(preprocessorSymbols);

        if (langPreview)
        {
            options = options.WithLanguageVersion(LanguageVersion.Preview);
        }

        return options;
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
        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
            _options.TryGetValue(key, out value);
    }

#if ROSLYN_4
    public static void AssertRunsEqual(
        GeneratorDriverRunResult runResult1,
        GeneratorDriverRunResult runResult2,
        IEnumerable<string> trackingNames)
    {
        // We're given all the tracking names, but not all the
        // stages will necessarily execute, so extract all the
        // output steps, and filter to ones we know about
        var trackedSteps1 = GetTrackedSteps(runResult1, trackingNames);
        var trackedSteps2 = GetTrackedSteps(runResult2, trackingNames);

        // Both runs should have the same tracked steps
        Assert.NotEmpty(trackedSteps1);
        Assert.Equal(trackedSteps1.Count, trackedSteps2.Count);
        Assert.Equal(trackedSteps1.Keys, trackedSteps2.Keys);

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
        Assert.Equal(runSteps1.Length, runSteps2.Length);

        for (var i = 0; i < runSteps1.Length; i++)
        {
            var runStep1 = runSteps1[i];
            var runStep2 = runSteps2[i];

            // The outputs should be equal between different runs
            var outputs1 = runStep1.Outputs.Select(x => x.Value);
            var outputs2 = runStep2.Outputs.Select(x => x.Value);

            WithMessage(() => Assert.Equal(outputs1, outputs2),
                $"because step {stepName} should produce cacheable outputs");

            // Therefore, on the second run the results should always be cached or unchanged!
            // - Unchanged is when the _input_ has changed, but the output hasn't
            // - Cached is when the the input has not changed, so the cached output is used
            IEnumerable<IncrementalStepRunReason> acceptableReasons =
                [IncrementalStepRunReason.Cached, IncrementalStepRunReason.Unchanged];
            foreach (var step in runStep2.Outputs)
            {
                Assert.Contains(step.Reason, acceptableReasons);
            }
        }
    }
#endif

    public static void WithMessage(Action action, string message)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            throw new AssertMessageException(message, ex);
        }
    }
}

[Serializable]
public class AssertMessageException(string message, Exception inner) : Exception(message, inner);

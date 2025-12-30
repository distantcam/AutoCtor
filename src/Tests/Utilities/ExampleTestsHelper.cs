using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TUnit.Core.Interfaces;

internal static class ExampleTestsHelper
{
    public static readonly IEnumerable<string> PreprocessorSymbols = [
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

    private static DirectoryInfo? BaseDir { get; } = new DirectoryInfo(Environment.CurrentDirectory).Parent?.Parent;

    public static IEnumerable<string> GetExamplesFiles(string path)
    {
        var examplesPath = Path.Combine(BaseDir?.FullName ?? "", path);
        if (!Directory.Exists(examplesPath))
            return [];
        return Directory.GetFiles(examplesPath, "*.cs")
            .Where(e => !e.Contains(".g.", StringComparison.InvariantCulture));
    }

    internal abstract class CompilationBuilderFactoryBase<TAttribute> : CompilationBuilderFactoryBase
    {
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);
            Builder = Builder.AddAssemblyReference<TAttribute>();
        }
    }

    internal abstract class CompilationBuilderFactoryBase : IAsyncInitializer
    {
        public CompilationBuilder Builder { get; protected set; } = null!;

        public virtual async Task InitializeAsync()
        {
            Builder = new CompilationBuilder()
                .WithNullableContextOptions(NullableContextOptions.Enable)
                .WithPreprocessorSymbols(PreprocessorSymbols);

            Builder = await Builder.AddNugetReference("Microsoft.NETCore.App.Ref", "ref")
                .ConfigureAwait(false);
            foreach (var id in GetNuGetIds())
            {
                Builder = await Builder.AddNugetReference(id)
                    .ConfigureAwait(false);
            }
        }

        protected virtual IEnumerable<string> GetNuGetIds() => [];

        public CompilationBuilder Create(CodeFileTheoryData theoryData)
        {
            var builder = Builder.AddCodes(theoryData.Codes);
            if (theoryData.LangPreview)
            {
                builder = builder.WithLanguageVersion(LanguageVersion.Preview);
            }
            return builder;
        }
    }

#if ROSLYN_4
    public static async Task AssertRunsEqual(
        GeneratorDriverRunResult runResult1,
        GeneratorDriverRunResult runResult2,
        IEnumerable<string> trackingNames)
    {
        // We're given all the tracking names, but not all the
        // stages will necessarily execute, so extract all the
        // output steps, and filter to ones we know about
        var trackedSteps1 = GetTrackedSteps(runResult1, trackingNames);
        var trackedSteps2 = GetTrackedSteps(runResult2, trackingNames);

        using var _ = Assert.Multiple();

        // Both runs should have the same tracked steps
        await Assert.That(trackedSteps1).IsNotEmpty()
            .ConfigureAwait(false);
        await Assert.That(trackedSteps2.Keys).IsEquivalentTo(trackedSteps1.Keys)
            .ConfigureAwait(false);

        // Get the IncrementalGeneratorRunStep collection for each run
        foreach (var (trackingName, runSteps1) in trackedSteps1)
        {
            // Assert that both runs produced the same outputs
            var runSteps2 = trackedSteps2[trackingName];
            await AssertStepsEqual(runSteps1, runSteps2)
                .ConfigureAwait(false);
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

    private static async Task AssertStepsEqual(
        ImmutableArray<IncrementalGeneratorRunStep> runSteps1,
        ImmutableArray<IncrementalGeneratorRunStep> runSteps2)
    {
        await Assert.That(runSteps2.Length).IsEqualTo(runSteps2.Length)
            .ConfigureAwait(false);

        for (var i = 0; i < runSteps1.Length; i++)
        {
            var runStep1 = runSteps1[i];
            var runStep2 = runSteps2[i];

            // The outputs should be equal between different runs
            var outputs1 = runStep1.Outputs.Select(x => x.Value);
            var outputs2 = runStep2.Outputs.Select(x => x.Value);

            await Assert.That(outputs2).IsEquivalentTo(outputs1)
                .ConfigureAwait(false);

            // Therefore, on the second run the results should always be cached or unchanged!
            // - Unchanged is when the _input_ has changed, but the output hasn't
            // - Cached is when the the input has not changed, so the cached output is used
            IEnumerable<IncrementalStepRunReason> acceptableReasons =
                [IncrementalStepRunReason.Cached, IncrementalStepRunReason.Unchanged];
            foreach (var (_, reason) in runStep2.Outputs)
            {
                await Assert.That(reason).IsIn(acceptableReasons)
                    .ConfigureAwait(false);
            }
        }
    }
#endif
}

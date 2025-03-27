using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

    public static CompilationBuilder CreateCompilation<TAssemblyReference>(CodeFileTheoryData theoryData)
    {
        var builder = new CompilationBuilder()
            .AddNetCoreReference()
            .AddAssemblyReference<TAssemblyReference>()
            .WithNullableContextOptions(NullableContextOptions.Enable)
            .WithPreprocessorSymbols(PreprocessorSymbols)
            .AddCodes(theoryData.Codes);

        if (theoryData.LangPreview)
        {
            builder = builder.WithLanguageVersion(LanguageVersion.Preview);
        }

        return builder;
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

    private static void WithMessage(Action action, string message)
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

    [Serializable]
    public class AssertMessageException(string message, Exception inner) : Exception(message, inner);
#endif
}

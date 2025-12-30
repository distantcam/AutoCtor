using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using TUnit.Assertions.Core;

internal static class TestHelper
{
    public static CancellationToken CancellationToken =>
        TestContext.Current?.Execution?.CancellationToken ?? CancellationToken.None;

    public static ISourceGenerator AsSourceGenerator(this ISourceGenerator generator) => generator;

    public static ConfiguredTaskAwaitable<TValue?> ConfigureAwait<TValue>(this Assertion<TValue> assertion, bool continueOnCapturedContext)
    {
        return assertion.AssertAsync().ConfigureAwait(continueOnCapturedContext);
    }
}

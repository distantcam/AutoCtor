using System.Runtime.CompilerServices;
using TUnit.Assertions.Core;

public static class TestHelper
{
    public static CancellationToken CancellationToken =>
        TestContext.Current?.Execution?.CancellationToken ?? CancellationToken.None;

    public static ConfiguredTaskAwaitable<TValue?> ConfigureAwait<TValue>(this Assertion<TValue> assertion, bool continueOnCapturedContext)
    {
        return assertion.AssertAsync().ConfigureAwait(continueOnCapturedContext);
    }
}

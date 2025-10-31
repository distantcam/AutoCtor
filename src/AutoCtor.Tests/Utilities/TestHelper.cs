public static class TestHelper
{
    public static CancellationToken CancellationToken =>
        TestContext.Current?.Execution?.CancellationToken ?? CancellationToken.None;
}

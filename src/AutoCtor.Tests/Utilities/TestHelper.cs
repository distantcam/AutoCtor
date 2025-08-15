public static class TestHelper
{
    public static CancellationToken CancellationToken =>
        TestContext.Current?.CancellationToken ?? CancellationToken.None;
}

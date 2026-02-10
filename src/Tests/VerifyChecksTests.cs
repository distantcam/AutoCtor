internal sealed class VerifyChecksTests
{
    [Test]
    [Skip("These should be more like guidelines instead of rules.")]
    public Task Run() =>
        VerifyChecks.Run();
}

using AutoCtor;

[AutoConstruct]
public partial class PostCtorWithOptionalArgumentsTest
{
    private readonly IA a;

    [AutoPostConstruct]
    private void Initialize(IB b = null)
    {
    }
}

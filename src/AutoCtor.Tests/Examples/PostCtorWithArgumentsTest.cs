using AutoCtor;

[AutoConstruct]
public partial class PostCtorWithArgumentsTest
{
    private readonly IA a;

    [AutoPostConstruct]
    private void Initialize(IB b)
    {
    }
}

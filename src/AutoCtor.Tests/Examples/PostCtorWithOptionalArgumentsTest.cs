[AutoCtor.AutoConstruct]
public partial class PostCtorWithOptionalArgumentsTest
{
    private readonly IA a;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(IB b = null)
    {
    }
}

[AutoCtor.AutoConstruct]
public partial class PostCtorWithGenericTest
{
    private readonly IA a;

    [AutoCtor.AutoPostConstruct]
    private void Initialize<T>(T t)
    {
    }
}

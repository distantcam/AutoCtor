using AutoCtor;

[AutoConstruct]
public partial class PostCtorWithGenericTest
{
    private readonly IA a;

    [AutoPostConstruct]
    private void Initialize<T>(T t)
    {
    }
}

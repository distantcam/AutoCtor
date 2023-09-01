using AutoCtor;

[AutoConstruct]
public partial class PostCtorAttributeTest
{
    private readonly IA a;

    [AutoPostConstruct]
    private void Initialize()
    {
    }
}

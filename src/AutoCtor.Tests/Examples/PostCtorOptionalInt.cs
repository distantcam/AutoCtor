[AutoCtor.AutoConstruct]
public partial class PostCtorOptionalInt
{
    [AutoCtor.AutoPostConstruct]
    private void Initialize(int value = 123, int df = default)
    {
    }
}

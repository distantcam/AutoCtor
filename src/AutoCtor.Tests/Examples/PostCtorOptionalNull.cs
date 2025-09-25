[AutoCtor.AutoConstruct]
public partial class PostCtorOptionalNull
{
    [AutoCtor.AutoPostConstruct]
    private void Initialize(string? value = null)
    {
    }
}

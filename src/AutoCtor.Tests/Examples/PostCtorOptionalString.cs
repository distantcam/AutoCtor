[AutoCtor.AutoConstruct]
public partial class PostCtorOptionalString
{
    [AutoCtor.AutoPostConstruct]
    private void Initialize(string value = "dfault")
    {
    }
}

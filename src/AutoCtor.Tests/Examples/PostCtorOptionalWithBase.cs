using AutoCtor;

[AutoConstruct]
public partial class PostCtorOptionalWithBase : BaseTest
{
    [AutoPostConstruct]
    private void Initialize(string value = "dfault")
    {
    }
}

[AutoConstruct]
public partial class BaseTest
{
    private readonly string _value;
}

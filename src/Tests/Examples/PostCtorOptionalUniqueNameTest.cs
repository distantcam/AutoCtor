using AutoCtor;

[AutoConstruct]
public partial class PostCtorOptionalUniqueNameTest
{
    private readonly string _value;

    [AutoPostConstruct]
    private void Initialise(string value = "")
    {
    }
}

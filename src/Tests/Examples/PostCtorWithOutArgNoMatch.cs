[AutoCtor.AutoConstruct]
public partial class PostCtorWithOutArgNoMatch
{
    [AutoCtor.AutoPostConstruct]
    private void Initialize(out string value)
    {
        value = "value should be flagged as not matching a valid field";
    }
}

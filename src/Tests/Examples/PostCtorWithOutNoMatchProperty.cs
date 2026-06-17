[AutoCtor.AutoConstruct]
public partial class PostCtorWithOutNoMatchProperty
{
    public string Value { get; }

    [AutoCtor.AutoPostConstruct]
    private void Initialize(out string value)
    {
        value = "value should be flagged as cannot match a property";
    }
}

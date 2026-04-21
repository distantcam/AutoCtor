public class ConstructorAssignsIgnoredField
{
    [AutoConstructIgnore]
    private readonly string _ignored;
    private readonly string _value;

    public ConstructorAssignsIgnoredField(string value, string ignored)
    {
        _value = value;
        _ignored = ignored;
    }
}

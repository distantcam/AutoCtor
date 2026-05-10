public class InitOnlyProperty
{
    public string Value { get; init; }

    public InitOnlyProperty(string value)
    {
        Value = value;
    }
}

public class ConstructorAssignsToMutableAutoProperty
{
    public string Value { get; set; }

    public ConstructorAssignsToMutableAutoProperty(string value)
    {
        Value = value;
    }
}

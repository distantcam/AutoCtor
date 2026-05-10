public class ConstructorAssignsToReadWriteProperty
{
    public string Value { get; private set; }

    public ConstructorAssignsToReadWriteProperty(string value)
    {
        Value = value;
    }
}

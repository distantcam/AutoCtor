public class MixedFieldsAndProperties
{
    private readonly string _value;
    public int Count { get; }

    public MixedFieldsAndProperties(string value, int count)
    {
        _value = value;
        Count = count;
    }
}

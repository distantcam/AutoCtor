public class BaseWithField
{
    protected string _baseValue = "";
}

public class ConstructorAssignsToBaseField : BaseWithField
{
    private readonly string _value;

    public ConstructorAssignsToBaseField(string value, string baseValue)
    {
        _value = value;
        _baseValue = baseValue;
    }
}

using AutoCtor;

public class IgnoredFieldIsNotAssigned
{
    [AutoConstructIgnore]
    private readonly string _ignored;
    private readonly string _value;

    public IgnoredFieldIsNotAssigned(string value)
    {
        _value = value;
    }
}

using System;

public class ConstructorWithAttribute
{
    private readonly string _value;

    [Obsolete]
    public ConstructorWithAttribute(string value)
    {
        _value = value;
    }
}

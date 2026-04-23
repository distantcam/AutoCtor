using System;

[Serializable]
public class ExistingAttribute
{
    private readonly string _value;

    public ExistingAttribute(string value)
    {
        _value = value;
    }
}

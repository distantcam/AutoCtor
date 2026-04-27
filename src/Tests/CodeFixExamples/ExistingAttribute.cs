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

[Serializable, Obsolete]
public class ExistingAttributeList
{
    private readonly string _value;

    public ExistingAttributeList(string value)
    {
        _value = value;
    }
}

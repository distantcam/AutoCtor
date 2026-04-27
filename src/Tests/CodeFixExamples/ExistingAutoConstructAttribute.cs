using System;
using AutoCtor;

[AutoCtor.AutoConstruct]
public class ExistingAutoConstructAttribute
{
    private readonly string _value;

    public ExistingAutoConstructAttribute(string value)
    {
        _value = value;
    }
}

[Serializable, AutoConstruct]
public class ExistingAutoConstructAttributeList
{
    private readonly string _value;

    public ExistingAutoConstructAttributeList(string value)
    {
        _value = value;
    }
}

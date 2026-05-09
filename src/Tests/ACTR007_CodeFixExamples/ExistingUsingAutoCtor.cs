#pragma warning disable CS8019 // Unnecessary using directive.

using AutoCtor;

public class ExistingUsingAutoCtor
{
    private readonly string _value;

    public ExistingUsingAutoCtor(string value)
    {
        _value = value;
    }
}

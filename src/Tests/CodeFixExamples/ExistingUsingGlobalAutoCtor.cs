#pragma warning disable CS8019 // Unnecessary using directive.

using global::AutoCtor;

public class ExistingUsingGlobalAutoCtor
{
    private readonly string _value;

    public ExistingUsingGlobalAutoCtor(string value)
    {
        _value = value;
    }
}

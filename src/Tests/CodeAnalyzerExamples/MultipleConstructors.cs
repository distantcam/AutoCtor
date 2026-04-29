public class MultipleConstructors
{
    private readonly string _value;

    public MultipleConstructors()
    {
        _value = string.Empty;
    }

    public MultipleConstructors(string value)
    {
        _value = value;
    }
}

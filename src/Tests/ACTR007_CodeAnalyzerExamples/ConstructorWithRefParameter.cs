public class ConstructorWithRefParameter
{
    private readonly string _value;

    public ConstructorWithRefParameter(ref string value)
    {
        _value = value;
    }
}

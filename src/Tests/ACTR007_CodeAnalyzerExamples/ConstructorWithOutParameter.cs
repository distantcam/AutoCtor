public class ConstructorWithOutParameter
{
    private readonly string _value;

    public ConstructorWithOutParameter(string value, out string result)
    {
        _value = value;
        result = value;
    }
}

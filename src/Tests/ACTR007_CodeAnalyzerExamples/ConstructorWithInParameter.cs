public class ConstructorWithInParameter
{
    private readonly int _value;

    public ConstructorWithInParameter(in int value)
    {
        _value = value;
    }
}

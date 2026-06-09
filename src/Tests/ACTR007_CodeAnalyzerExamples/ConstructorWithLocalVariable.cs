public class ConstructorWithLocalVariable
{
    private readonly string _value;

    public ConstructorWithLocalVariable(string value)
    {
        var temp = value.Trim();
        _value = temp;
    }
}

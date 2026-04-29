public class ConstructorWithNonAssignment
{
    private readonly string _value;

    public ConstructorWithNonAssignment(string value)
    {
        _value = value;
        System.Console.WriteLine(value);
    }
}

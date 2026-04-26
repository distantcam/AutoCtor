public class ConstructorWithExplicitDefault
{
    private readonly string _value;
    private readonly string? _optional;

    public ConstructorWithExplicitDefault(string value, string? optional = default)
    {
        _value = value;
        _optional = optional;
    }
}

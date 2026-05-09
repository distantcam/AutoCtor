public class FieldHasInitializer
{
    private readonly string _value = "default";

    public FieldHasInitializer(string value)
    {
        _value = value;
    }
}

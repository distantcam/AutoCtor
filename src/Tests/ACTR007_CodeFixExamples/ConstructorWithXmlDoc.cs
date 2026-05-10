public class ConstructorWithXmlDoc
{
    private readonly string _value;

    /// <summary>Constructs an instance.</summary>
    public ConstructorWithXmlDoc(string value)
    {
        _value = value;
    }
}

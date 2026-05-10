public class HasThisInitializer
{
    private readonly string _value;

    public HasThisInitializer() { }

    public HasThisInitializer(string value) : this()
    {
        _value = value;
    }
}

public abstract class Base
{
    protected Base(string name) { }
}

public class HasBaseInitializer : Base
{
    private readonly string _value;

    public HasBaseInitializer(string name, string value) : base(name)
    {
        _value = value;
    }
}

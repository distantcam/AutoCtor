public class ConstructorAssignsMethodCall
{
    private readonly string _value;

    public ConstructorAssignsMethodCall(string value)
    {
        _value = value.ToUpper();
    }
}

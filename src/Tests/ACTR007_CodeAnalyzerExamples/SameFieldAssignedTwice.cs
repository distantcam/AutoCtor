public class SameFieldAssignedTwice
{
    private readonly string _value;

    public SameFieldAssignedTwice(string a)
    {
        _value = a;
        _value = a;
    }
}

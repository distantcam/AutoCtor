public class SameParamUsedTwice
{
    private readonly string _a;
    private readonly string _b;

    public SameParamUsedTwice(string value)
    {
        _a = value;
        _b = value;
    }
}

public class ConstructorAssignsNonParameter
{
    private static readonly string s_default = "default";
    private readonly string _value;

    public ConstructorAssignsNonParameter(string value)
    {
        _value = s_default;
    }
}

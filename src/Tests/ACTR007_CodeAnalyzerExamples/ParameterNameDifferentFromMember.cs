public class ParameterNameDifferentFromMember
{
    private readonly string _value;

    public ParameterNameDifferentFromMember(string name)
    {
        _value = name;
    }
}

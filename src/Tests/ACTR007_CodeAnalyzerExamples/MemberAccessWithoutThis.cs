public class MemberAccessWithoutThis
{
    private static int s_count;
    private readonly int _value;

    public MemberAccessWithoutThis(int value)
    {
        MemberAccessWithoutThis.s_count = 1;
        _value = value;
    }
}

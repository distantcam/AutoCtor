public class PrimaryConstructorPresent(int initial)
{
    private readonly string _other;
    private readonly int _value = initial;

    public PrimaryConstructorPresent(int initial, string other) : this(initial)
    {
        _other = other;
    }
}

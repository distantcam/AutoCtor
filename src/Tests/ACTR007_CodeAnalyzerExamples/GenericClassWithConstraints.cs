public class GenericClassWithConstraints<T> where T : class
{
    private readonly T _value;

    public GenericClassWithConstraints(T value)
    {
        _value = value;
    }
}

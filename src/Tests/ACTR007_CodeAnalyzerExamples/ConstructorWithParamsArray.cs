public class ConstructorWithParamsArray
{
    private readonly int[] _values;

    public ConstructorWithParamsArray(params int[] values)
    {
        _values = values;
    }
}

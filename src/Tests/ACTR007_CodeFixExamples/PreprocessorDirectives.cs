public class PreprocessorDirectives
{
    private readonly string _value;

#if !DEBUG
    public PreprocessorDirectives(string value)
    {
        _value = value;
    }
#endif
}

public partial class MultiplePartialFiles
{
    private readonly string _value;

    public MultiplePartialFiles(string value)
    {
        _value = value;
    }
}

public partial class MultiplePartialFiles
{
    public void DoSomething() { }
}

using AutoCtor;

public class CustomBase
{
    public CustomBase(string text)
    {
    }
}

[AutoConstruct]
public partial class BaseTest : CustomBase
{
    private readonly int _value;
}

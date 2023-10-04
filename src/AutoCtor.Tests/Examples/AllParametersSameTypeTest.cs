using AutoCtor;

[AutoConstruct]
public partial class AllParametersAreSameTypeTest : BaseClass
{
    private readonly IA a;

    [AutoPostConstruct]
    private void Initialize(IA a)
    {
    }
}

public abstract class BaseClass
{
    public BaseClass(IA a)
    {
    }
}

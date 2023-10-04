using AutoCtor;

[AutoConstruct]
public partial class UniqueNameTest : BaseClass
{
    private readonly IA a;

    [AutoPostConstruct]
    private void Initialize(IB a)
    {
    }
}

public abstract class BaseClass
{
    public BaseClass(IC a)
    {
    }
}

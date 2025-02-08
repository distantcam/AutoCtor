public interface IServiceA { }
public interface IServiceB { }
public interface IServiceC { }

[AutoCtor.AutoConstruct]
public partial class UniqueNameTest : BaseClass
{
    private readonly IServiceA _serviceA;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(IServiceB serviceB)
    {
    }
}

public abstract class BaseClass
{
    public BaseClass(IServiceC serviceC)
    {
    }
}

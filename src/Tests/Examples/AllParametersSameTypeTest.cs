public interface IService { }

[AutoCtor.AutoConstruct]
public partial class AllParametersAreSameTypeTest : BaseClass
{
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(IService service)
    {
    }
}

public abstract class BaseClass
{
    public BaseClass(IService baseService)
    {
    }
}

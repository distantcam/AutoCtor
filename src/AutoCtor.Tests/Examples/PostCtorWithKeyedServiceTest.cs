public interface IService { }

[AutoCtor.AutoConstruct]
public partial class BaseClass
{
    [AutoCtor.FromKeyedServices("base")]
    private readonly IService _service;
}

[AutoCtor.AutoConstruct]
public partial class PostCtorWithKeyedServiceTest : BaseClass
{
    [AutoCtor.FromKeyedServices("field")]
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize([AutoCtor.FromKeyedServices("postconstruct")] IService postConstructService)
    {
    }
}

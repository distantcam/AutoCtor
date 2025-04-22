public interface IService { }

[AutoCtor.AutoConstruct]
public partial class BaseClass
{
    [AutoCtor.AutoKeyedService("base")]
    private readonly IService _service;
}

[AutoCtor.AutoConstruct]
public partial class PostCtorWithKeyedServiceTest : BaseClass
{
    [AutoCtor.AutoKeyedService("field")]
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize([AutoCtor.AutoKeyedService("postconstruct")] IService postConstructService)
    {
    }
}

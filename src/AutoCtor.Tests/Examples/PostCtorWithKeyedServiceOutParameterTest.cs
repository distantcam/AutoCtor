public interface IService { }

[AutoCtor.AutoConstruct]
public partial class PostCtorWithKeyedServiceOutParameterTest
{
    [AutoCtor.FromKeyedServices("keyed")]
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(out IService service)
    {
        service = new Service();
    }

    private class Service : IService { }
}

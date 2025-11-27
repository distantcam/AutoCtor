public interface IService { }

[AutoCtor.AutoConstruct]
public partial class PostCtorWithKeyedOutParameterTest
{
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize([AutoCtor.AutoKeyedService("keyed")] out IService service)
    {
        service = new Service();
    }

    private class Service : IService { }
}

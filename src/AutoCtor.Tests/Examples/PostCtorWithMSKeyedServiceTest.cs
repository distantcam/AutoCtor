public interface IService { }

[AutoCtor.AutoConstruct]
public partial class PostCtorWithMSKeyedServiceTest
{
    [AutoCtor.AutoKeyedService("field")]
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(
        [Microsoft.Extensions.DependencyInjection.FromKeyedServices("postconstruct")]
        IService postConstructService)
    {
    }
}

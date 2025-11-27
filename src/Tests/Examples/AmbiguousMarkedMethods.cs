public interface IService { }
public interface IAnotherService { }

[AutoCtor.AutoConstruct]
public partial class AmbiguousMarkedMethods
{
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize()
    {
    }

    [AutoCtor.AutoPostConstruct]
    private void Initialize(IAnotherService anotherService)
    {
    }
}

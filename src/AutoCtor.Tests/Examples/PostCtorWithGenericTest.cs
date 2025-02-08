public interface IService { }

[AutoCtor.AutoConstruct]
public partial class PostCtorWithGenericTest
{
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize<T>(T t)
    {
    }
}

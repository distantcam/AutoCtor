public interface IService { }

[AutoCtor.AutoConstruct]
public partial class PostCtorAttributeTest
{
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize()
    {
    }
}

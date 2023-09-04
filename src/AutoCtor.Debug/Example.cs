using AutoCtor;

public interface IService { }

[AutoConstruct]
public partial class MyClass
{
    private readonly IService _service;

    [AutoPostConstruct]
    private void Initialize() { }
}

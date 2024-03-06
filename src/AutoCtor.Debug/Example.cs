using AutoCtor;

public interface IService { }

[AutoConstruct(guard: GuardSetting.Enabled)]
public partial class MyClass
{
    private readonly IService _service;

    [AutoPostConstruct]
    private void Initialize() { }
}

public interface IService { }

[AutoCtor.AutoConstruct]
public partial class OtherClass
{
}

[AutoCtor.AutoConstruct]
public partial class Broken251Release
{
    private readonly IService _service;
}

using AutoCtor;

public interface IService;

#region PostConstruct

[AutoConstruct]
public partial class PostConstruct
{
    private readonly IService _service;

    [AutoPostConstruct]
    private void Initialize()
    {
    }
}

#endregion

using AutoCtor;

public interface IService;
public interface IInitializeService;

#region PostConstructWithParameter

[AutoConstruct]
public partial class PostConstructWithParameter
{
    private readonly IService _service;

    [AutoPostConstruct]
    private void Initialize(IInitializeService initialiseService)
    {
    }
}

#endregion

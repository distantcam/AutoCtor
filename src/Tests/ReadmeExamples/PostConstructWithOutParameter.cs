using AutoCtor;

public interface IService;
public interface IOtherService;
public interface IServiceProvider { TService GetService<TService>(); }

#region PostConstructWithOutParameter

[AutoConstruct]
public partial class PostConstructWithOutParameter
{
    private readonly IService _service;
    private readonly IOtherService _otherService;

    [AutoPostConstruct]
    private void Initialize(IServiceProvider services, out IService service)
    {
        service = services.GetService<IService>();
    }
}

#endregion

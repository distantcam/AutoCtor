namespace AutoCtor.PostConstructExamples;

public interface IService;
public interface IOtherService;
public interface IInitialiseService;
public interface IServiceFactory { IService CreateService(); }

#pragma warning disable IDE0040 // Add accessibility modifiers

#region PostConstruct

[AutoConstruct]
public partial class PostConstructMethod
{
    private readonly IService _service;

    [AutoPostConstruct]
    private void Initialize()
    {
    }
}

#endregion

partial class PostConstructMethod
{
    #region PostConstructGeneratedCode
    public PostConstructMethod(IService service)
    {
        _service = service;
        Initialize();
    }
    #endregion
}

#region PostConstructWithParameters

public partial class PostConstructMethodWithParameters
{
    private readonly IService _service;

    [AutoPostConstruct]
    private void Initialize(IInitialiseService initialiseService)
    {
    }
}

#endregion

partial class PostConstructMethodWithParameters
{
    #region PostConstructWithParametersGeneratedCode
    public PostConstructMethodWithParameters(IService service, IInitialiseService initialiseService)
    {
        _service = service;
        Initialize(initialiseService);
    }
    #endregion
}

#region PostConstructWithOutParameters

public partial class PostConstructWithOutParameters
{
    private readonly IService _service;
    private readonly IOtherService _otherService;

    [AutoPostConstruct]
    private void Initialize(IServiceFactory serviceFactory, out IService service)
    {
        service = serviceFactory.CreateService();
    }
}

#endregion

partial class PostConstructWithOutParameters
{
    #region PostConstructWithOutParametersGeneratedCode
    public PostConstructWithOutParameters(IOtherService otherService, IServiceFactory serviceFactory)
    {
        _otherService = otherService;
        Initialize(serviceFactory, out _service);
    }
    #endregion
}

namespace AutoCtor.PostConstructExamples;

public interface IInitialiseService { }

#region NamedPostConstruct

[AutoConstruct(nameof(Initialize))]
public partial class NamedPostConstruct
{
    private readonly IService _service;

    private void Initialize()
    {
    }
}

#endregion

#region NamedPostConstructGeneratedCode

partial class NamedPostConstruct
{
    public NamedPostConstruct(IService service)
    {
        _service = service;
        Initialize();
    }
}

#endregion

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

#region PostConstructGeneratedCode

partial class PostConstructMethod
{
    public PostConstructMethod(IService service)
    {
        _service = service;
        Initialize();
    }
}

#endregion

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

#region PostConstructWithParametersGeneratedCode

partial class PostConstructMethodWithParameters
{
    public PostConstructMethodWithParameters(IService service, IInitialiseService initialiseService)
    {
        _service = service;
        Initialize(initialiseService);
    }
}

#endregion

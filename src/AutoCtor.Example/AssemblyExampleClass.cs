using AutoCtor;

#region AssemblyAttribute

[assembly: AutoConstruct("Initialise")]

[AutoConstruct]
public partial class AssemblyAttributeExample
{
    private readonly IService _service;

    private void Initialize()
    {
    }
}

#endregion

#region AssemblyAttributeGeneratedCode

partial class AssemblyAttributeExample
{
    public AssemblyAttributeExample(IService service)
    {
        _service = service;
        Initialize();
    }
}

#endregion

public interface IService { }

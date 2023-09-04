namespace AutoCtor.BasicExamples;

public interface IService { }
public interface IAnotherService { }

#region Basic

[AutoConstruct]
public partial class ExampleClass
{
    private readonly IService _service;
}

#endregion

#region BasicGeneratedCode

partial class ExampleClass
{
    public ExampleClass(IService service)
    {
        _service = service;
    }
}

#endregion

#region PresetField

[AutoConstruct]
public partial class ClassWithPresetField
{
    private readonly IService _service;
    private readonly IList<string> _list = new List<string>();
}

#endregion

#region PresetFieldGeneratedCode

partial class ClassWithPresetField
{
    public ClassWithPresetField(IService service)
    {
        _service = service;
        // no code to set _list
    }
}

#endregion

#region Inherit

public abstract class BaseClass
{
    protected IAnotherService _anotherService;

    public BaseClass(IAnotherService anotherService)
    {
        _anotherService = anotherService;
    }
}

[AutoConstruct]
public partial class ClassWithBase : BaseClass
{
    private readonly IService _service;
}

#endregion

#region InheritGeneratedCode

partial class ClassWithBase
{
    public ClassWithBase(IAnotherService anotherService, IService service) : base(anotherService)
    {
        _service = service;
    }
}

#endregion

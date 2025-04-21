namespace AutoCtor.BasicExamples;

public interface IService;
public interface IAnotherService;

#region Basic

[AutoConstruct]
public partial class ExampleClass
{
    private readonly IService _service;
}

#endregion

partial class ExampleClass
{
    #region BasicGeneratedCode
    public ExampleClass(IService service)
    {
        _service = service;
    }
    #endregion
}

#region PresetField

[AutoConstruct]
public partial class ClassWithPresetField
{
    private readonly IService _service;
    private readonly IList<string> _list = new List<string>();
}

#endregion

partial class ClassWithPresetField
{
    #region PresetFieldGeneratedCode
    public ClassWithPresetField(IService service)
    {
        _service = service;
        // no code to set _list
    }
    #endregion
}

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

partial class ClassWithBase
{
    #region InheritGeneratedCode
    public ClassWithBase(IAnotherService anotherService, IService service) : base(anotherService)
    {
        _service = service;
    }
    #endregion
}

public partial class PropertyExamples
{
    #region PropertyExamples

    // AutoCtor will initialise these
    public string GetProperty { get; }
    protected string ProtectedProperty { get; }
    public string InitProperty { get; init; }
    public required string RequiredProperty { get; set; }

    // AutoCtor will ignore these
    public string InitializerProperty { get; } = "Constant";
    public string GetSetProperty { get; set; }
    public string FixedProperty => "Constant";
    public string RedirectedProperty => InitializerProperty;

    #endregion
}

#region KeyedService

[AutoConstruct]
public partial class KeyedExampleClass
{
    [FromKeyedServices("key")]
    private readonly IService _keyedService;
}

#endregion

partial class KeyedExampleClass
{
    #region KeyedServiceGeneratedCode
    public KeyedExampleClass(
        [Microsoft.Extensions.DependencyInjection.FromKeyedServices("key")]
        IService keyedService)
    {
        _keyedService = keyedService;
    }
    #endregion
}

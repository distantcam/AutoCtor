namespace AutoCtor.Example;

public interface ICustomService { }
public interface IAnotherService { }

#region YourCode

[AutoConstruct]
public partial class ExampleClass
{
    private readonly ICustomService _customService;
}

#endregion

#region GeneratedCode

partial class ExampleClass
{
    public ExampleClass(ICustomService customService)
    {
        _customService = customService;
    }
}

#endregion

#region ExampleWithInitializer

[AutoConstruct]
public partial class ClassWithInitializer
{
    private readonly ICustomService _customService;
    private readonly IList<string> _list = new List<string>();
}

#endregion

#region ExampleWithInitializerGeneratedCode

partial class ClassWithInitializer
{
    public ClassWithInitializer(ICustomService customService)
    {
        _customService = customService;
        // no code to set _list
    }
}

#endregion

#region ExampleWithBase

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
    private readonly ICustomService _customService;
}

#endregion

#region ExampleWithBaseGeneratedCode

partial class ClassWithBase
{
    public ClassWithBase(IAnotherService anotherService, ICustomService customService) : base(anotherService)
    {
        _customService = customService;
    }
}

#endregion

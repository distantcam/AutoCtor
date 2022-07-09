namespace AutoCtor.Example;

public interface ICustomService { }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class AutoConstruct : Attribute { }

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

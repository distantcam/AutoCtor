namespace AutoCtor.Example;

public interface ICustomService { }

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

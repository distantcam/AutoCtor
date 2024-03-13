using AutoCtor;

public class Service { }

#region Guards

[AutoConstruct(GuardSetting.Enabled)]
public partial class GuardedClass
{
    private readonly Service _service;
}

#endregion

#region GuardsGeneratedCode

partial class GuardedClass
{
    public GuardedClass(Service service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }
}

#endregion

using AutoCtor;

public class Service { }

#region Guards

[AutoConstruct(GuardSetting.Enabled)]
public partial class GuardedClass
{
    private readonly Service _service;
}

#endregion


partial class GuardedClass
{
    #region GuardsGeneratedCode
    public GuardedClass(Service service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }
    #endregion
}

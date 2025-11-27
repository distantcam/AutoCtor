using AutoCtor;

public interface IService;

#region Guarded

[AutoConstruct(GuardSetting.Enabled)]
public partial class Guarded
{
    private readonly IService _service;
}

#endregion

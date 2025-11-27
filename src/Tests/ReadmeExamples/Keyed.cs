using AutoCtor;

public interface IService;

#region Keyed

[AutoConstruct]
public partial class Keyed
{
    [AutoKeyedService("key")]
    private readonly IService _keyedService;
}

#endregion

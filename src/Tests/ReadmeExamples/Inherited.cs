using AutoCtor;

public interface IService;
public interface IAnotherService;

#region Inherited

public abstract class BaseClass
{
    protected IAnotherService _anotherService;

    public BaseClass(IAnotherService anotherService)
    {
        _anotherService = anotherService;
    }
}

[AutoConstruct]
public partial class Inherited : BaseClass
{
    private readonly IService _service;
}

#endregion

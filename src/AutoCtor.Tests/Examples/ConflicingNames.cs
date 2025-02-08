public interface IServiceA { }
public interface IServiceB { }
public interface IServiceC { }

[AutoCtor.AutoConstruct]
public partial class AClass
{
    private readonly IServiceA _service;
}

[AutoCtor.AutoConstruct]
public partial class BClass : AClass
{
    private readonly IServiceB _service;
}

[AutoCtor.AutoConstruct]
public partial class CClass : BClass
{
    private readonly IServiceC _service;
}

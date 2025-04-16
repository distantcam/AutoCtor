public interface IService { }
public interface IService<T> { }

public enum Keys
{
    Default
}

[AutoCtor.AutoConstruct]
public partial class KeyedServicesTest<T>
{
    [AutoCtor.FromKeyedServicesAttribute(null)]
    private readonly IService _null;
    [AutoCtor.FromKeyedServicesAttribute("key")]
    private readonly IService _string;
    [AutoCtor.FromKeyedServicesAttribute(0)]
    private readonly IService _int;
    [AutoCtor.FromKeyedServicesAttribute(true)]
    private readonly IService _bool;
    [AutoCtor.FromKeyedServicesAttribute(Keys.Default)]
    private readonly IService _enum;
    [AutoCtor.FromKeyedServicesAttribute("generic")]
    private readonly IService<T> _generic;
    private readonly IService _notKeyed;
}

[AutoCtor.AutoConstruct]
public partial class ChildKeyedServicesTest : KeyedServicesTest<int>
{
}

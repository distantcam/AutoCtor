public interface IService { }
public interface IService<T> { }

public enum Keys
{
    Default
}

[AutoCtor.AutoConstruct]
public partial class KeyedServicesTest<T>
{
    [AutoCtor.AutoKeyedServiceAttribute(null)]
    private readonly IService _null;
    [AutoCtor.AutoKeyedServiceAttribute("key")]
    private readonly IService _string;
    [AutoCtor.AutoKeyedServiceAttribute(0)]
    private readonly IService _int;
    [AutoCtor.AutoKeyedServiceAttribute(true)]
    private readonly IService _bool;
    [AutoCtor.AutoKeyedServiceAttribute(Keys.Default)]
    private readonly IService _enum;
    [AutoCtor.AutoKeyedServiceAttribute("generic")]
    private readonly IService<T> _generic;
    private readonly IService _notKeyed;
}

[AutoCtor.AutoConstruct]
public partial class ChildKeyedServicesTest : KeyedServicesTest<int>
{
}

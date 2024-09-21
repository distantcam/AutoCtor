public interface ILogger<T1>
{
}

[AutoCtor.AutoConstruct]
public partial class Base<T2>
{
    protected ILogger<Base<T2>> Logger { get; }
}

[AutoCtor.AutoConstruct]
public partial class Derived : Base<int>
{
}

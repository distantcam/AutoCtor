public interface ILogger<T>
{
}

public class Base<TLogger>
{
    public Base(ILogger<TLogger> logger)
    {
    }
}

[AutoCtor.AutoConstruct]
public partial class Derived : Base<Derived>
{
    private readonly ILogger<Derived> logger;
}

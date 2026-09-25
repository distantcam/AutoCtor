using AutoCtor;

// The rule's lifetime carries to every construction made from it.
[ServiceProvider]
[Transient(typeof(IFactory<>), typeof(Factory<>))]
[Singleton<IBuilder, Builder>]
public partial class FactoryProvider;

public class Part;

public interface IFactory<T>;
public class Factory<T> : IFactory<T>;

public interface IBuilder;

public class Builder : IBuilder
{
    public Builder(IFactory<Part> factory) { }
}

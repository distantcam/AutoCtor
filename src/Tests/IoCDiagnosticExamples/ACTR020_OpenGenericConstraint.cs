using AutoCtor;

// IRef<int> is asked for, but closing RefImpl<> over int breaks its class constraint. The
// generated code would not compile, so it is caught here instead.
[ServiceProvider]
[Singleton(typeof(IRef<>), typeof(RefImpl<>))]
[Singleton<IRefConsumer, RefConsumer>]
public partial class RefProvider;

public interface IRef<T>;
public class RefImpl<T> : IRef<T> where T : class;

public interface IRefConsumer;

public class RefConsumer : IRefConsumer
{
    public RefConsumer(IRef<int> numbers) { }
}

using AutoCtor;

// An open registration is closed on demand, so there is no one type for a factory to build.
[ServiceProvider]
[Singleton(typeof(IRepository<>), typeof(Repository<>), Factory = nameof(CreateRepository))]
public sealed partial class OpenGenericFactoryProvider
{
    private Repository<int>? CreateRepository() => null;
}

public interface IRepository<T>;
public class Repository<T> : IRepository<T>;

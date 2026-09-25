using AutoCtor;

// The provider gains CreateScope and a nested Scope type; the scoped service is built
// eagerly by the scope, not the provider.
[ServiceProvider]
[Singleton<IAppConfig, AppConfig>]
[Scoped<IUnitOfWork, UnitOfWork>]
public partial class ScopedSimpleProvider;

public interface IAppConfig;
public class AppConfig : IAppConfig;

public interface IUnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    public UnitOfWork(IAppConfig config) { }
}

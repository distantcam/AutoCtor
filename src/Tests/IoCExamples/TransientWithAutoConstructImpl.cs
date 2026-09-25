using AutoCtor;

// As with singletons, the implementation's constructor does not exist yet when the IoC
// stage runs, so the factory has to use the constructor AutoCtor is about to generate.
[ServiceProvider]
[Singleton<ITrAcDependency, TrAcDependency>]
[Transient<ITrAcService, TrAcService>]
public partial class TransientAutoConstructProvider;

public interface ITrAcDependency;
public class TrAcDependency : ITrAcDependency;

public interface ITrAcService;

[AutoConstruct]
public partial class TrAcService : ITrAcService
{
    private readonly ITrAcDependency _dependency;
}

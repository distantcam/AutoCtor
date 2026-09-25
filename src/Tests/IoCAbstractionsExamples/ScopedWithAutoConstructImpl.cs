using AutoCtor;

// The scoped implementation's constructor does not exist yet when the IoC stage runs.
[ServiceProvider]
[Singleton<IScAcDependency, ScAcDependency>]
[Scoped<IScAcService, ScAcService>]
public partial class ScopedAutoConstructProvider;

public interface IScAcDependency;
public class ScAcDependency : IScAcDependency;

public interface IScAcService;

[AutoConstruct]
public partial class ScAcService : IScAcService
{
    private readonly IScAcDependency _dependency;
}

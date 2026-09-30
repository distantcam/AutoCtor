using AutoCtor;

// The implementation's constructor does not exist yet when the IoC stage runs, so the
// emitter has to use the constructor AutoCtor is about to generate.
[ServiceProvider]
[Singleton<IAcDependency, AcDependency>]
[Singleton<IAcService, AcService>]
public sealed partial class AutoConstructImplProvider;

public interface IAcDependency;
public class AcDependency : IAcDependency;

public interface IAcService;

[AutoConstruct]
public partial class AcService : IAcService
{
    private readonly IAcDependency _dependency;
}

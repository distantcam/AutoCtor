using AutoCtor;

[ServiceProvider]
[Singleton<IChainDepOne, ChainDepOne>]
[Singleton<IChainDepTwo, ChainDepTwo>]
[Singleton<IDerivedService, DerivedService>]
public sealed partial class AutoConstructChainProvider;

public interface IChainDepOne;
public interface IChainDepTwo;
public class ChainDepOne : IChainDepOne;
public class ChainDepTwo : IChainDepTwo;

public interface IDerivedService;

[AutoConstruct]
public partial class BaseService
{
    private readonly IChainDepOne _one;
}

[AutoConstruct]
public partial class DerivedService : BaseService, IDerivedService
{
    private readonly IChainDepTwo _two;
}

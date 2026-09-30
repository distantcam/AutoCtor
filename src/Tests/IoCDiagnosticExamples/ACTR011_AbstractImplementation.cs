using AutoCtor;

[ServiceProvider]
[Singleton(typeof(IAbstractService), typeof(AbstractService))]
public sealed partial class AbstractImplProvider;

public interface IAbstractService;
public abstract class AbstractService : IAbstractService;

using AutoCtor;

[ServiceProvider]
[Singleton(typeof(IAbstractService), typeof(AbstractService))]
public partial class AbstractImplProvider;

public interface IAbstractService;
public abstract class AbstractService : IAbstractService;

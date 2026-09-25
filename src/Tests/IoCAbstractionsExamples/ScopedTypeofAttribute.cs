using AutoCtor;

[ServiceProvider]
[Scoped(typeof(IScopedTypeofService), typeof(ScopedTypeofService))]
public partial class ScopedTypeofAttributeProvider;

public interface IScopedTypeofService;
public class ScopedTypeofService : IScopedTypeofService;

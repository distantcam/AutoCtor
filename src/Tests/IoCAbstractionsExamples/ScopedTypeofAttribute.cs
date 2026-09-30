using AutoCtor;

[ServiceProvider]
[Scoped(typeof(IScopedTypeofService), typeof(ScopedTypeofService))]
public sealed partial class ScopedTypeofAttributeProvider;

public interface IScopedTypeofService;
public class ScopedTypeofService : IScopedTypeofService;

using AutoCtor;

[ServiceProvider]
[Singleton(typeof(IKeyedTypeofService), typeof(KeyedTypeofService), Key = "alpha")]
public sealed partial class KeyedTypeofProvider;

public interface IKeyedTypeofService;
public class KeyedTypeofService : IKeyedTypeofService;

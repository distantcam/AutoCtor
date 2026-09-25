using AutoCtor;

// An explicit closed registration takes precedence over the rule for that construction,
// so IStore<Special> uses SpecialStore rather than a synthesised Store<Special>.
[ServiceProvider]
[Singleton(typeof(IStore<>), typeof(Store<>))]
[Singleton<IStore<Special>, SpecialStore>]
[Singleton<IStoreHost, StoreHost>]
public partial class StoreProvider;

public class Special;
public class Ordinary;

public interface IStore<T>;
public class Store<T> : IStore<T>;
public class SpecialStore : IStore<Special>;

public interface IStoreHost;

public class StoreHost : IStoreHost
{
    public StoreHost(IStore<Special> special, IStore<Ordinary> ordinary) { }
}

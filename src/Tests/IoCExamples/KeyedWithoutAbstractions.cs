using AutoCtor;

// Keyed registrations without the Microsoft package. GetKeyedService and
// GetRequiredKeyedService are ordinary public methods, so they stand on their own;
// only IKeyedServiceProvider is skipped, along with KeyedService.AnyKey, which is a
// value that lives in the package rather than anything registered here.
[ServiceProvider]
[Singleton<IStore, LocalStore>]
[Singleton<IStore, RemoteStore>(Key = "remote")]
public sealed partial class StoreProvider;

public interface IStore;
public class LocalStore : IStore;
public class RemoteStore : IStore;

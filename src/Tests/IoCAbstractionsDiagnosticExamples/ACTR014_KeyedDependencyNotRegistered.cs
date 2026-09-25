using AutoCtor;

// The service is registered, just not under the key being asked for.
[ServiceProvider]
[Singleton<IStore, PrimaryStore>(Key = "primary")]
[Singleton<IStoreClient, StoreClient>]
public partial class StoreProvider;

public interface IStore;
public class PrimaryStore : IStore;

public interface IStoreClient;

public class StoreClient : IStoreClient
{
    public StoreClient([AutoKeyedService("secondary")] IStore store) { }
}

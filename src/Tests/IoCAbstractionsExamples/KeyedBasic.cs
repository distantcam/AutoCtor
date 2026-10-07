using AutoCtor;

// Keys make separate slots, so the same service can have an unkeyed registration and one
// per key. Each keyed winner gets its own getter named after the key.
[ServiceProvider]
[Singleton<ICache, DefaultCache>]
[Singleton<ICache, RedisCache>(Key = "redis")]
[Singleton<ICache, MemoryCache>(Key = "memory")]
public sealed partial class CacheProvider;

public interface ICache;
public class DefaultCache : ICache;
public class RedisCache : ICache;
public class MemoryCache : ICache;

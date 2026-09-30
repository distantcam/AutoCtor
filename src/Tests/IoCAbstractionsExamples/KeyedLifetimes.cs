using AutoCtor;

// A key is orthogonal to lifetime -- each keyed slot keeps its own.
[ServiceProvider]
[Singleton<IChannel, CachedChannel>(Key = "cached")]
[Transient<IChannel, FreshChannel>(Key = "fresh")]
[Scoped<IChannel, ScopedChannel>(Key = "scoped")]
public sealed partial class ChannelProvider;

public interface IChannel;
public class CachedChannel : IChannel;
public class FreshChannel : IChannel;
public class ScopedChannel : IChannel;

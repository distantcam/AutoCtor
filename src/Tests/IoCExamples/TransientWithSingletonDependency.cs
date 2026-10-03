using AutoCtor;

// A transient may freely depend on a singleton -- the getter reads the backing field
// every time it builds a new instance.
[ServiceProvider]
[Singleton<ISharedConfig, SharedConfig>]
[Transient<IRequestHandler, RequestHandler>]
public sealed partial class TransientDependencyProvider;

public interface ISharedConfig;
public class SharedConfig : ISharedConfig;

public interface IRequestHandler;

public class RequestHandler : IRequestHandler
{
    public RequestHandler(ISharedConfig config) { }
}

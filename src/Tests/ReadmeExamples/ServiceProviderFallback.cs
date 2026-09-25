using AutoCtor;

public interface ILogSink;
public interface IGreeter;
public class Greeter(ILogSink log) : IGreeter
{
    public ILogSink Log => log;
}

#region ServiceProviderFallback

[ServiceProvider(Fallback = nameof(_host))]
[Singleton<IGreeter, Greeter>]
public partial class FallbackContainer
{
    private readonly System.IServiceProvider _host;

    public FallbackContainer(System.IServiceProvider host) => _host = host;
}

#endregion

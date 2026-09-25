using AutoCtor;
using Microsoft.Extensions.DependencyInjection;

// Registering IServiceScopeFactory yourself, when the provider already answers it. The
// registration wins, and only one resolver is emitted for the type.
[ServiceProvider]
[Singleton<ILogSink, LogSink>]
[Singleton<IServiceScopeFactory, CustomScopeFactory>]
[Scoped<IRequestState, RequestState>]
public partial class ScopeFactoryProvider;

public interface ILogSink;
public class LogSink : ILogSink;

public interface IRequestState;

public class RequestState : IRequestState
{
    public RequestState(ILogSink sink) { }
}

public class CustomScopeFactory : IServiceScopeFactory
{
    public IServiceScope CreateScope() => null!;
}

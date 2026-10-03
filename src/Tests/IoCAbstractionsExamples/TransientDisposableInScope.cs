using AutoCtor;
using System;

// RequestLog needs nothing scoped, so a transient that was not disposable would live on the
// provider with the scope forwarding to it. Because it is disposable the scope builds its own
// instead: forwarding would leave every scope's instances owned by the root, and alive until
// the provider is. Audit depends on a scoped service, so it only ever exists on the scope.
[ServiceProvider]
[Singleton<IClock, Clock>]
[Scoped<IRequest, Request>]
[Transient<IRequestLog, RequestLog>]
[Transient<IAudit, Audit>]
public sealed partial class RequestProvider;

public interface IClock;
public class Clock : IClock;

// Disposable and scoped, so the scope disposes it by the compile-time list before it drains
// the transients it handed out.
public interface IRequest;
public class Request : IRequest, IDisposable
{
    public Request(IClock clock) { }
    public void Dispose() { }
}

public interface IRequestLog;
public class RequestLog : IRequestLog, IDisposable
{
    public RequestLog(IClock clock) { }
    public void Dispose() { }
}

public interface IAudit;
public class Audit : IAudit, IDisposable
{
    public Audit(IRequest request) { }
    public void Dispose() { }
}

using System;
using AutoCtor;
using Microsoft.Extensions.DependencyInjection;

// Where the abstractions package is present the fallback answers the keyed questions too,
// and the IsService probes forward to it -- a host asks those before it resolves, so a
// fallback that answered GetService and not IsService would never be reached through one.
// The member is declared on the provider, so the scope reaches it through _root.
//
// A keyed constructor dependency the fallback answers is why UnitOfWork compiles: nothing
// here is registered under "audit", so the call goes to GetRequiredKeyedService, which is
// emitted for it even though every keyed chain on this provider is otherwise empty.
[ServiceProvider(Fallback = nameof(Host))]
[Singleton<IHandler, PrimaryHandler>(Key = "primary")]
[Scoped<IUnitOfWork, UnitOfWork>]
public partial class DispatchProvider
{
    public IServiceProvider Host { get; }

    public DispatchProvider(IServiceProvider host) => Host = host;
}

public interface IHandler;
public class PrimaryHandler : IHandler;
public interface IAuditSink;
public interface IUnitOfWork;
public class UnitOfWork([FromKeyedServices("audit")] IAuditSink sink) : IUnitOfWork
{
    public IAuditSink Sink => sink;
}

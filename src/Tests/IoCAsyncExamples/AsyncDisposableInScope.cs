using AutoCtor;
using System;
using System.Threading.Tasks;

// A scope always implements IServiceScope, and so always has a Dispose. Session can only be
// released asynchronously, so that Dispose names DisposeAsync instead of skipping it, and the
// scope picks up IAsyncDisposable. Only transients are tracked in the list, so a scoped async
// service does not stop it being a list of IDisposable.
[ServiceProvider]
[Singleton<IClock, Clock>]
[Scoped<ISession, Session>]
[Transient<ILease, Lease>]
public sealed partial class SessionProvider;

public interface IClock;
public class Clock : IClock;

public interface ISession;
public class Session : ISession, IAsyncDisposable
{
    public Session(IClock clock) { }
    public ValueTask DisposeAsync() => default;
}

public interface ILease;
public class Lease : ILease, IDisposable
{
    public Lease(ISession session) { }
    public void Dispose() { }
}

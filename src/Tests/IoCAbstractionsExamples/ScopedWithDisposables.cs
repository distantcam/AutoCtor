using AutoCtor;
using System;

// Which cached implementations are disposable is known at compile time, so disposal is a
// direct call list in reverse construction order, with no runtime tracking. Nothing here is
// a disposable transient, so no provider carries a stack -- see TransientDisposableInScope.
[ServiceProvider]
[Singleton<IPoolOwner, PoolOwner>]
[Scoped<ISession, Session>]
[Scoped<ITracker, Tracker>]
public sealed partial class DisposableProvider;

public interface IPoolOwner;
public class PoolOwner : IPoolOwner, IDisposable
{
    public void Dispose() { }
}

public interface ISession;
public class Session : ISession, IDisposable
{
    public Session(IPoolOwner owner) { }
    public void Dispose() { }
}

// Not disposable, so it is skipped entirely.
public interface ITracker;
public class Tracker : ITracker
{
    public Tracker(ISession session) { }
}

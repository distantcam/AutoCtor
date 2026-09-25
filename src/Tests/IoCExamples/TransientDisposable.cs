using AutoCtor;
using System;

// A transient is disposed by whoever handed it out, so the provider tracks the ones it builds
// and drains that stack when it is disposed. Only the disposable transient is tracked -- the
// other stays a bare factory call and costs nothing.
[ServiceProvider]
[Singleton<IClock, Clock>]
[Transient<IConnection, Connection>]
[Transient<IFormatter, Formatter>]
public partial class ConnectionProvider;

public interface IClock;
public class Clock : IClock;

public interface IConnection;
public class Connection : IConnection, IDisposable
{
    public Connection(IClock clock) { }
    public void Dispose() { }
}

public interface IFormatter;
public class Formatter : IFormatter
{
    public Formatter(IClock clock) { }
}

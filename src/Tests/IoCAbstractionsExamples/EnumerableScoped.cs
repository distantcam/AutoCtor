using AutoCtor;
using System.Collections.Generic;

// One scoped element makes the whole collection scope bound, so it lives on the Scope and
// the provider does not expose it at all.
[ServiceProvider]
[Singleton<IListener, GlobalListener>]
[Scoped<IListener, RequestListener>]
[Scoped<IDispatcher, Dispatcher>]
public partial class DispatcherProvider;

public interface IListener;
public class GlobalListener : IListener;
public class RequestListener : IListener;

public interface IDispatcher;

public class Dispatcher : IDispatcher
{
    public Dispatcher(IEnumerable<IListener> listeners) { }
}

using AutoCtor;

// A scan for an open generic finds every construction of it, and registers each type under
// the closed service it implements.
[ServiceProvider]
[ScanTransient(typeof(IHandler<>))]
[Transient<IDispatcher, Dispatcher>]
public sealed partial class HandlerProvider;

public interface IHandler<TMessage>;

public class CreateOrder;
public class CancelOrder;

public class CreateOrderHandler : IHandler<CreateOrder>;
public class CancelOrderHandler : IHandler<CancelOrder>;

public interface IDispatcher;

public class Dispatcher : IDispatcher
{
    public Dispatcher(IHandler<CreateOrder> create, IHandler<CancelOrder> cancel) { }
}

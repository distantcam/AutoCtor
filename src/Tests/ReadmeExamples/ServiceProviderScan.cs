using AutoCtor;

public class CreateOrder;
public class CancelOrder;

#region ServiceProviderScan

[ServiceProvider]
[ScanTransient(typeof(IHandler<>))]
public sealed partial class OrderContainer;

public interface IHandler<TMessage>;

public class CreateOrderHandler : IHandler<CreateOrder>;
public class CancelOrderHandler : IHandler<CancelOrder>;

#endregion

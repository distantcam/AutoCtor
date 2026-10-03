using AutoCtor;

// TypeNameFilter narrows a scan to types whose name matches, where * matches anything.
[ServiceProvider]
[ScanScoped(TypeNameFilter = "*Repository", As = ScanAs.ImplementedInterfaces)]
public sealed partial class DataStoreProvider;

public interface ICustomerRepository;
public interface IOrderRepository;
public class CustomerRepository : ICustomerRepository;
public class OrderRepository : IOrderRepository;
public class RepositoryHelper : IOrderRepository;

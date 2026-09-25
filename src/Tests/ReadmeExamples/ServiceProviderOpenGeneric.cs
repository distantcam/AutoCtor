using AutoCtor;

public interface ISession;
public class Session : ISession;

public class Customer;

#region ServiceProviderOpenGeneric

[ServiceProvider]
[Singleton<ISession, Session>]
[Singleton(typeof(IRepository<>), typeof(Repository<>))]
[Singleton<ICustomerReport, CustomerReport>]
public partial class ReportContainer;

public interface IRepository<T>;

public class Repository<T> : IRepository<T>
{
    public Repository(ISession session) { }
}

public interface ICustomerReport;

public class CustomerReport : ICustomerReport
{
    public CustomerReport(IRepository<Customer> customers) { }
}

#endregion

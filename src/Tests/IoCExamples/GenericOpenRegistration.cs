using AutoCtor;

// The open registration is a rule. Only IRepository<User> is injected anywhere, so that is
// the single construction emitted -- IRepository<Order> is never asked for.
[ServiceProvider]
[Singleton<IDbSession, DbSession>]
[Singleton(typeof(IRepository<>), typeof(Repository<>))]
[Singleton<IUserReport, UserReport>]
public sealed partial class RepositoryProvider;

public class User;
public class Order;

public interface IDbSession;
public class DbSession : IDbSession;

public interface IRepository<T>;

public class Repository<T> : IRepository<T>
{
    public Repository(IDbSession session) { }
}

public interface IUserReport;

public class UserReport : IUserReport
{
    public UserReport(IRepository<User> users) { }
}

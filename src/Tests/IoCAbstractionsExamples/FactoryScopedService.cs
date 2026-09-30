using AutoCtor;

// A scoped service is built inside the scope, where the factory member is not: it is
// declared on the provider, so the scope calls it through _root while the arguments still
// resolve against the scope.
[ServiceProvider]
[Scoped<ISession, Session>]
[Scoped<IUnitOfWork>(Factory = nameof(CreateUnitOfWork))]
public sealed partial class UnitOfWorkProvider
{
    private IUnitOfWork CreateUnitOfWork(ISession session) => new UnitOfWork(session);
}

public interface ISession;
public class Session : ISession;
public interface IUnitOfWork;
public class UnitOfWork(ISession session) : IUnitOfWork
{
    public ISession Session => session;
}

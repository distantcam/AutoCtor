using AutoCtor;

// A factory parameter typed IServiceProvider is handed the container the service is being
// built in -- the provider for the singleton below, the scope for the scoped one. It is the
// only parameter that does not come from a registration.
[ServiceProvider]
[Singleton<IRegistry>(Factory = nameof(CreateRegistry))]
[Scoped<IAudit>(Factory = nameof(CreateAudit))]
public partial class AuditProvider
{
    private IRegistry CreateRegistry(System.IServiceProvider services) => new Registry(services);

    private IAudit CreateAudit(System.IServiceProvider services) => new Audit(services);
}

public interface IRegistry;
public class Registry(System.IServiceProvider services) : IRegistry
{
    public System.IServiceProvider Services => services;
}
public interface IAudit;
public class Audit(System.IServiceProvider services) : IAudit
{
    public System.IServiceProvider Services => services;
}

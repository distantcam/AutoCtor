using AutoCtor;

// The provider outlives every scope it creates, so a singleton can never hold a scoped
// service. The second case reaches the scoped service through a transient, which is the
// same capture one step removed.
[ServiceProvider]
[Scoped<ITenantContext, TenantContext>]
[Singleton<ICacheDirect, CacheDirect>]
[Transient<IQuery, Query>]
[Singleton<ICacheIndirect, CacheIndirect>]
public sealed partial class ScopedCaptureProvider;

public interface ITenantContext;
public class TenantContext : ITenantContext;

public interface ICacheDirect;
public class CacheDirect : ICacheDirect
{
    public CacheDirect(ITenantContext context) { }
}

public interface IQuery;
public class Query : IQuery
{
    public Query(ITenantContext context) { }
}

public interface ICacheIndirect;
public class CacheIndirect : ICacheIndirect
{
    public CacheIndirect(IQuery query) { }
}

using AutoCtor;
using System;

public interface IAppSettings;
public interface IDbContext;

public class AppSettings : IAppSettings;

#region ServiceProviderScoped

[ServiceProvider]
[Singleton<IAppSettings, AppSettings>]
[Scoped<IDbContext, DbContext>]
public sealed partial class RequestContainer;

public class DbContext : IDbContext, IDisposable
{
    public DbContext(IAppSettings settings) { }
    public void Dispose() { }
}

#endregion

using AutoCtor;

public interface IClock;
public interface IAuditLog;
public interface IBanner;

public class SystemClock(string timeZone) : IClock
{
    public string TimeZone => timeZone;
}
public class AuditLog(System.IServiceProvider services) : IAuditLog
{
    public System.IServiceProvider Services => services;
}
public class Banner(string text) : IBanner
{
    public string Text => text;
}

#region ServiceProviderFactory

[ServiceProvider]
[Singleton<IClock>(Factory = nameof(CreateClock))]
[Singleton<IBanner>(Factory = nameof(_banner))]
[Scoped<IAuditLog>(Factory = nameof(CreateAuditLog))]
public partial class HostContainer
{
    private readonly IBanner _banner = new Banner("AutoCtor");

    private IClock CreateClock() => new SystemClock("UTC");

    private IAuditLog CreateAuditLog(System.IServiceProvider services) => new AuditLog(services);
}

#endregion

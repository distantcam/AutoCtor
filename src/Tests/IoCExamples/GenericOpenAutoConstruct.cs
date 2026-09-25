using AutoCtor;

// The open implementation is [AutoConstruct], so its constructor does not exist yet AND
// its parameters have to be substituted for the closed construction.
[ServiceProvider]
[Singleton<IClock, SystemClock>]
[Singleton(typeof(ICache<>), typeof(Cache<>))]
[Singleton<IReportService, ReportService>]
public partial class CacheProvider;

public class Report;

public interface IClock;
public class SystemClock : IClock;

public interface ICache<T>;

[AutoConstruct]
public partial class Cache<T> : ICache<T>
{
    private readonly IClock _clock;
}

public interface IReportService;

public class ReportService : IReportService
{
    public ReportService(ICache<Report> cache) { }
}

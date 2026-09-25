using AutoCtor;

// Nothing here is scoped and nothing references Microsoft.Extensions.DependencyInjection,
// and the provider still has CreateScope. The scope is an IServiceProvider that is also
// IDisposable, so `using var scope = provider.CreateScope()` compiles either way; the
// Microsoft interfaces are added on top of that shape wherever the reference exists.
[ServiceProvider]
[Singleton<IClock, Clock>]
[Transient<IJob, Job>]
public partial class ScopelessProvider;

public interface IClock;
public class Clock : IClock;

public interface IJob;

public class Job : IJob
{
    public Job(IClock clock) { }
}

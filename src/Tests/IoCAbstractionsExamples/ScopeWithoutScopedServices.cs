using AutoCtor;

// No [Scoped] registration, and the provider is still an IServiceScopeFactory handing back
// a full IServiceScope. Anything that already understands Microsoft's container can take
// this one, whatever it happens to have registered. The scope owns nothing of its own here,
// so every getter reads through to the provider -- except the disposable transient, which
// whoever handed it out is the one to dispose.
[ServiceProvider]
[Singleton<IIndex, SearchIndex>]
[Transient<ILease, Lease>]
public partial class IndexProvider;

public interface IIndex;
public class SearchIndex : IIndex;

public interface ILease;

public class Lease : ILease, System.IDisposable
{
    public Lease(IIndex index) { }
    public void Dispose() { }
}

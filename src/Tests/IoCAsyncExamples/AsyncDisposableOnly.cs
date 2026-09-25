using AutoCtor;
using System;
using System.Threading.Tasks;

// Nothing here can be disposed synchronously, so the provider does not implement IDisposable
// at all. Reaching for `using` is then a compile error naming the missing interface, rather
// than a Dispose that exists only to throw.
[ServiceProvider]
[Singleton<IStream, Stream>]
public partial class StreamProvider;

public interface IStream;
public class Stream : IStream, IAsyncDisposable
{
    public ValueTask DisposeAsync() => default;
}

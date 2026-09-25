using AutoCtor;
using System;
using System.Threading.Tasks;

// Three shapes at once. Cache implements both interfaces, so Dispose uses the synchronous one
// and DisposeAsync the asynchronous one. Pump implements only the asynchronous one, so Dispose
// cannot release it and says so rather than passing over it. Channel is a transient that is
// only asynchronously disposable, which is what makes the tracking list hold objects rather
// than IDisposable -- the two interfaces have to be told apart when it is walked.
[ServiceProvider]
[Singleton<ICache, Cache>]
[Singleton<IPump, Pump>]
[Transient<IChannel, Channel>]
public partial class MediaProvider;

public interface ICache;
public class Cache : ICache, IDisposable, IAsyncDisposable
{
    public void Dispose() { }
    public ValueTask DisposeAsync() => default;
}

public interface IPump;
public class Pump : IPump, IAsyncDisposable
{
    public Pump(ICache cache) { }
    public ValueTask DisposeAsync() => default;
}

public interface IChannel;
public class Channel : IChannel, IAsyncDisposable
{
    public Channel(ICache cache) { }
    public ValueTask DisposeAsync() => default;
}

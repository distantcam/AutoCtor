using System;
using AutoCtor;

// A fallback is read on the miss path, where there is nothing to resolve a parameter from.
[ServiceProvider(Fallback = nameof(GetHost))]
[Singleton<IService, Service>]
public partial class ParameterisedFallbackProvider
{
    private IServiceProvider GetHost(int tenant) => throw new NotSupportedException();
}

public interface IService;
public class Service : IService;

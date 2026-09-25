using BenchmarkDotNet.Attributes;
using Jab;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks;

[BenchmarkCategory(Categories.Singleton)]
[ShortRunJob]
[MemoryDiagnoser]
public partial class BasicSingletonBenchmark
{
    // One loop length. The 100 and 1000 runs measured the same per-resolve cost, and
    // resolving one, two or three services differed from each other by a multiplier.
    private const int GetCount = 1000;

    private readonly ServiceProvider _provider;
    private readonly JabSingletonProvider _jabProvider = new();
    private readonly AutoCtorProvider _autoCtorProvider = new();

    public BasicSingletonBenchmark()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ISingleton1, Singleton1>();
        serviceCollection.AddSingleton<ISingleton2, Singleton2>();
        serviceCollection.AddSingleton<ISingleton3, Singleton3>();
        _provider = serviceCollection.BuildServiceProvider();
    }

    [BenchmarkCategory(Categories.MEDI)]
    [Benchmark]
    public void MEDI()
    {
        for (var i = 0; i < GetCount; i++)
        {
            _provider.GetService<ISingleton1>();
            _provider.GetService<ISingleton2>();
            _provider.GetService<ISingleton3>();
        }
    }

    [BenchmarkCategory(Categories.Jab)]
    [Benchmark(Baseline = true)]
    public void Jab()
    {
        for (var i = 0; i < GetCount; i++)
        {
            _jabProvider.GetService<ISingleton1>();
            _jabProvider.GetService<ISingleton2>();
            _jabProvider.GetService<ISingleton3>();
        }
    }

    [BenchmarkCategory(Categories.AutoCtor)]
    [Benchmark]
    public void AutoCtor()
    {
        for (var i = 0; i < GetCount; i++)
        {
            _autoCtorProvider.GetService<ISingleton1>();
            _autoCtorProvider.GetService<ISingleton2>();
            _autoCtorProvider.GetService<ISingleton3>();
        }
    }

    [ServiceProvider]
    [Singleton(typeof(ISingleton1), typeof(Singleton1))]
    [Singleton(typeof(ISingleton2), typeof(Singleton2))]
    [Singleton(typeof(ISingleton3), typeof(Singleton3))]
    private partial class JabSingletonProvider;

    [AutoCtor.ServiceProvider]
    [AutoCtor.Singleton(typeof(ISingleton1), typeof(Singleton1))]
    [AutoCtor.Singleton(typeof(ISingleton2), typeof(Singleton2))]
    [AutoCtor.Singleton(typeof(ISingleton3), typeof(Singleton3))]
    private partial class AutoCtorProvider;
}

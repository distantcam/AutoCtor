using BenchmarkDotNet.Attributes;
using Jab;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks;

[BenchmarkCategory(Categories.Transient)]
[ShortRunJob]
[MemoryDiagnoser]
public partial class BasicTransientBenchmark
{
    // One loop length. The 100 and 1000 runs measured the same per-resolve cost, and
    // resolving one, two or three services differed from each other by a multiplier.
    private const int GetCount = 1000;

    private readonly ServiceProvider _provider;
    private readonly JabTransientProvider _jabProvider = new();
    private readonly AutoCtorProvider _autoCtorProvider = new();

    public BasicTransientBenchmark()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<ITransient1, Transient1>();
        serviceCollection.AddTransient<ITransient2, Transient2>();
        serviceCollection.AddTransient<ITransient3, Transient3>();
        _provider = serviceCollection.BuildServiceProvider();
    }

    [BenchmarkCategory(Categories.MEDI)]
    [Benchmark]
    public void MEDI()
    {
        for (var i = 0; i < GetCount; i++)
        {
            _provider.GetService<ITransient1>();
            _provider.GetService<ITransient2>();
            _provider.GetService<ITransient3>();
        }
    }

    [BenchmarkCategory(Categories.Jab)]
    [Benchmark(Baseline = true)]
    public void Jab()
    {
        for (var i = 0; i < GetCount; i++)
        {
            _jabProvider.GetService<ITransient1>();
            _jabProvider.GetService<ITransient2>();
            _jabProvider.GetService<ITransient3>();
        }
    }

    [BenchmarkCategory(Categories.AutoCtor)]
    [Benchmark]
    public void AutoCtor()
    {
        for (var i = 0; i < GetCount; i++)
        {
            _autoCtorProvider.GetService<ITransient1>();
            _autoCtorProvider.GetService<ITransient2>();
            _autoCtorProvider.GetService<ITransient3>();
        }
    }

    [ServiceProvider]
    [Transient(typeof(ITransient1), typeof(Transient1))]
    [Transient(typeof(ITransient2), typeof(Transient2))]
    [Transient(typeof(ITransient3), typeof(Transient3))]
    private partial class JabTransientProvider;

    [AutoCtor.ServiceProvider]
    [AutoCtor.Transient(typeof(ITransient1), typeof(Transient1))]
    [AutoCtor.Transient(typeof(ITransient2), typeof(Transient2))]
    [AutoCtor.Transient(typeof(ITransient3), typeof(Transient3))]
    private partial class AutoCtorProvider;
}

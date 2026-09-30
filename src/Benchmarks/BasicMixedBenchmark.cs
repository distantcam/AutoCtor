using BenchmarkDotNet.Attributes;
using Jab;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks;

[BenchmarkCategory(Categories.Mixed)]
[ShortRunJob]
[MemoryDiagnoser]
public partial class BasicMixedBenchmark
{
    // One loop length. The 100 and 1000 runs measured the same per-resolve cost, and
    // resolving one, two or three services differed from each other by a multiplier.
    private const int GetCount = 1000;

    private readonly ServiceProvider _provider;
    private readonly JabMixProvider _jabProvider = new();
    private readonly AutoCtorProvider _autoCtorProvider = new();

    public BasicMixedBenchmark()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IMix1, Mix1>();
        serviceCollection.AddTransient<IMix2, Mix2>();
        serviceCollection.AddTransient<IMix3, Mix3>();
        serviceCollection.AddTransient<ITransient1, Transient1>();
        serviceCollection.AddTransient<ITransient2, Transient2>();
        serviceCollection.AddTransient<ITransient3, Transient3>();
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
            using var scope = _provider.CreateScope();

            scope.ServiceProvider.GetService<IMix1>();
            scope.ServiceProvider.GetService<IMix2>();
            scope.ServiceProvider.GetService<IMix3>();
        }
    }

    [BenchmarkCategory(Categories.Jab)]
    [Benchmark(Baseline = true)]
    public void Jab()
    {
        for (var i = 0; i < GetCount; i++)
        {
            using var scope = _jabProvider.CreateScope();

            scope.GetService<IMix1>();
            scope.GetService<IMix2>();
            scope.GetService<IMix3>();
        }
    }

    [BenchmarkCategory(Categories.AutoCtor)]
    [Benchmark]
    public void AutoCtor()
    {
        for (var i = 0; i < GetCount; i++)
        {
            using var scope = _autoCtorProvider.CreateScope();

            scope.GetService<IMix1>();
            scope.GetService<IMix2>();
            scope.GetService<IMix3>();
        }
    }

    [ServiceProvider]
    [Transient(typeof(IMix1), typeof(Mix1))]
    [Transient(typeof(IMix2), typeof(Mix2))]
    [Transient(typeof(IMix3), typeof(Mix3))]
    [Transient(typeof(ITransient1), typeof(Transient1))]
    [Transient(typeof(ITransient2), typeof(Transient2))]
    [Transient(typeof(ITransient3), typeof(Transient3))]
    [Singleton(typeof(ISingleton1), typeof(Singleton1))]
    [Singleton(typeof(ISingleton2), typeof(Singleton2))]
    [Singleton(typeof(ISingleton3), typeof(Singleton3))]
    private sealed partial class JabMixProvider;

    [AutoCtor.ServiceProvider]
    [AutoCtor.Transient(typeof(IMix1), typeof(Mix1))]
    [AutoCtor.Transient(typeof(IMix2), typeof(Mix2))]
    [AutoCtor.Transient(typeof(IMix3), typeof(Mix3))]
    [AutoCtor.Transient(typeof(ITransient1), typeof(Transient1))]
    [AutoCtor.Transient(typeof(ITransient2), typeof(Transient2))]
    [AutoCtor.Transient(typeof(ITransient3), typeof(Transient3))]
    [AutoCtor.Singleton(typeof(ISingleton1), typeof(Singleton1))]
    [AutoCtor.Singleton(typeof(ISingleton2), typeof(Singleton2))]
    [AutoCtor.Singleton(typeof(ISingleton3), typeof(Singleton3))]
    private sealed partial class AutoCtorProvider;
}

using BenchmarkDotNet.Attributes;
using Jab;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks;

[BenchmarkCategory(Categories.Scoped)]
[ShortRunJob]
[MemoryDiagnoser]
public partial class BasicScopedBenchmark
{
    // One loop length. The 100 and 1000 runs measured the same per-resolve cost, and
    // resolving one, two or three services differed from each other by a multiplier.
    private const int GetCount = 1000;

    private readonly ServiceProvider _provider;
    private readonly JabScopedProvider _jabProvider = new();
    private readonly AutoCtorProvider _autoCtorProvider = new();

    public BasicScopedBenchmark()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IScoped1, Scoped1>();
        serviceCollection.AddScoped<IScoped2, Scoped2>();
        serviceCollection.AddScoped<IScoped3, Scoped3>();
        _provider = serviceCollection.BuildServiceProvider();
    }

    [BenchmarkCategory(Categories.MEDI)]
    [Benchmark]
    public void MEDI()
    {
        for (var i = 0; i < GetCount; i++)
        {
            using var scope = _provider.CreateScope();

            scope.ServiceProvider.GetService<IScoped1>();
            scope.ServiceProvider.GetService<IScoped2>();
            scope.ServiceProvider.GetService<IScoped3>();
        }
    }

    [BenchmarkCategory(Categories.Jab)]
    [Benchmark(Baseline = true)]
    public void Jab()
    {
        for (var i = 0; i < GetCount; i++)
        {
            using var scope = _jabProvider.CreateScope();

            scope.GetService<IScoped1>();
            scope.GetService<IScoped2>();
            scope.GetService<IScoped3>();
        }
    }

    [BenchmarkCategory(Categories.AutoCtor)]
    [Benchmark]
    public void AutoCtor()
    {
        for (var i = 0; i < GetCount; i++)
        {
            using var scope = _autoCtorProvider.CreateScope();

            scope.GetService<IScoped1>();
            scope.GetService<IScoped2>();
            scope.GetService<IScoped3>();
        }
    }

    [ServiceProvider]
    [Scoped(typeof(IScoped1), typeof(Scoped1))]
    [Scoped(typeof(IScoped2), typeof(Scoped2))]
    [Scoped(typeof(IScoped3), typeof(Scoped3))]
    internal partial class JabScopedProvider;

    [AutoCtor.ServiceProvider]
    [AutoCtor.Scoped(typeof(IScoped1), typeof(Scoped1))]
    [AutoCtor.Scoped(typeof(IScoped2), typeof(Scoped2))]
    [AutoCtor.Scoped(typeof(IScoped3), typeof(Scoped3))]
    internal partial class AutoCtorProvider;
}

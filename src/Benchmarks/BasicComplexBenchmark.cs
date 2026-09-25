using BenchmarkDotNet.Attributes;
using Jab;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks;

[BenchmarkCategory(Categories.Complex)]
[ShortRunJob]
[MemoryDiagnoser]
public partial class BasicComplexBenchmark
{
    // One loop length. The 100 and 1000 runs measured the same per-resolve cost, and
    // resolving one, two or three services differed from each other by a multiplier.
    private const int GetCount = 1000;

    private readonly ServiceProvider _provider;
    private readonly JabComplexProvider _jabProvider = new();
    private readonly AutoCtorProvider _autoCtorProvider = new();

    public BasicComplexBenchmark()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IComplex1, Complex1>();
        serviceCollection.AddScoped<IComplex2, Complex2>();
        serviceCollection.AddScoped<IComplex3, Complex3>();
        serviceCollection.AddTransient<IService1, Service1>();
        serviceCollection.AddTransient<IService2, Service2>();
        serviceCollection.AddTransient<IService3, Service3>();
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

            scope.ServiceProvider.GetService<IComplex1>();
            scope.ServiceProvider.GetService<IComplex2>();
            scope.ServiceProvider.GetService<IComplex3>();
        }
    }

    [BenchmarkCategory(Categories.Jab)]
    [Benchmark(Baseline = true)]
    public void Jab()
    {
        for (var i = 0; i < GetCount; i++)
        {
            using var scope = _jabProvider.CreateScope();

            scope.GetService<IComplex1>();
            scope.GetService<IComplex2>();
            scope.GetService<IComplex3>();
        }
    }

    [BenchmarkCategory(Categories.AutoCtor)]
    [Benchmark]
    public void AutoCtor()
    {
        for (var i = 0; i < GetCount; i++)
        {
            using var scope = _autoCtorProvider.CreateScope();

            scope.GetService<IComplex1>();
            scope.GetService<IComplex2>();
            scope.GetService<IComplex3>();
        }
    }

    [ServiceProvider]
    [Scoped(typeof(IComplex1), typeof(Complex1))]
    [Scoped(typeof(IComplex2), typeof(Complex2))]
    [Scoped(typeof(IComplex3), typeof(Complex3))]
    [Transient(typeof(IService1), typeof(Service1))]
    [Transient(typeof(IService2), typeof(Service2))]
    [Transient(typeof(IService3), typeof(Service3))]
    [Transient(typeof(IMix1), typeof(Mix1))]
    [Transient(typeof(IMix2), typeof(Mix2))]
    [Transient(typeof(IMix3), typeof(Mix3))]
    [Transient(typeof(ITransient1), typeof(Transient1))]
    [Transient(typeof(ITransient2), typeof(Transient2))]
    [Transient(typeof(ITransient3), typeof(Transient3))]
    [Singleton(typeof(ISingleton1), typeof(Singleton1))]
    [Singleton(typeof(ISingleton2), typeof(Singleton2))]
    [Singleton(typeof(ISingleton3), typeof(Singleton3))]
    private partial class JabComplexProvider;

    [AutoCtor.ServiceProvider]
    [AutoCtor.Scoped(typeof(IComplex1), typeof(Complex1))]
    [AutoCtor.Scoped(typeof(IComplex2), typeof(Complex2))]
    [AutoCtor.Scoped(typeof(IComplex3), typeof(Complex3))]
    [AutoCtor.Transient(typeof(IService1), typeof(Service1))]
    [AutoCtor.Transient(typeof(IService2), typeof(Service2))]
    [AutoCtor.Transient(typeof(IService3), typeof(Service3))]
    [AutoCtor.Transient(typeof(IMix1), typeof(Mix1))]
    [AutoCtor.Transient(typeof(IMix2), typeof(Mix2))]
    [AutoCtor.Transient(typeof(IMix3), typeof(Mix3))]
    [AutoCtor.Transient(typeof(ITransient1), typeof(Transient1))]
    [AutoCtor.Transient(typeof(ITransient2), typeof(Transient2))]
    [AutoCtor.Transient(typeof(ITransient3), typeof(Transient3))]
    [AutoCtor.Singleton(typeof(ISingleton1), typeof(Singleton1))]
    [AutoCtor.Singleton(typeof(ISingleton2), typeof(Singleton2))]
    [AutoCtor.Singleton(typeof(ISingleton3), typeof(Singleton3))]
    private partial class AutoCtorProvider;
}

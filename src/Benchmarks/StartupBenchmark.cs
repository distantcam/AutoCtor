using BenchmarkDotNet.Attributes;
using Jab;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks;

// What it costs to stand a container up, and nothing else. Every method returns the
// provider rather than resolving from it: BenchmarkDotNet consumes the returned value, so
// nothing is optimised away, and no service is constructed. Resolving anything here would
// fold a lookup into the number -- and for AutoCtor and Jab that lookup is far larger than
// the construction it would be hiding inside.
[ShortRunJob]
[MemoryDiagnoser]
public partial class StartupBenchmark
{
    [BenchmarkCategory(Categories.Singleton)]
    [Benchmark]
    public IServiceProvider Jab_Singleton() => new JabStartupSingleton();

    [BenchmarkCategory(Categories.Singleton)]
    [Benchmark]
    public IServiceProvider AutoCtor_Singleton() => new AutoCtorStartupSingleton();

    [BenchmarkCategory(Categories.Singleton)]
    [Benchmark]
    public IServiceProvider MEDI_Singleton()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ISingleton1, Singleton1>();
        serviceCollection.AddSingleton<ISingleton2, Singleton2>();
        serviceCollection.AddSingleton<ISingleton3, Singleton3>();
        return serviceCollection.BuildServiceProvider();
    }

    [ServiceProvider]
    [Singleton(typeof(ISingleton1), typeof(Singleton1))]
    [Singleton(typeof(ISingleton2), typeof(Singleton2))]
    [Singleton(typeof(ISingleton3), typeof(Singleton3))]
    private partial class JabStartupSingleton;

    [AutoCtor.ServiceProvider]
    [AutoCtor.Singleton(typeof(ISingleton1), typeof(Singleton1))]
    [AutoCtor.Singleton(typeof(ISingleton2), typeof(Singleton2))]
    [AutoCtor.Singleton(typeof(ISingleton3), typeof(Singleton3))]
    private partial class AutoCtorStartupSingleton;

    // ------------------------------------------------------------------------

    [BenchmarkCategory(Categories.Scoped)]
    [Benchmark]
    public IServiceProvider Jab_Scoped() => new JabStartupScoped();

    [BenchmarkCategory(Categories.Scoped)]
    [Benchmark]
    public IServiceProvider AutoCtor_Scoped() => new AutoCtorStartupScoped();

    [BenchmarkCategory(Categories.Scoped)]
    [Benchmark]
    public IServiceProvider MEDI_Scoped()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IScoped1, Scoped1>();
        serviceCollection.AddScoped<IScoped2, Scoped2>();
        serviceCollection.AddScoped<IScoped3, Scoped3>();
        return serviceCollection.BuildServiceProvider();
    }

    [ServiceProvider]
    [Scoped(typeof(IScoped1), typeof(Scoped1))]
    [Scoped(typeof(IScoped2), typeof(Scoped2))]
    [Scoped(typeof(IScoped3), typeof(Scoped3))]
    private partial class JabStartupScoped;

    [AutoCtor.ServiceProvider]
    [AutoCtor.Scoped(typeof(IScoped1), typeof(Scoped1))]
    [AutoCtor.Scoped(typeof(IScoped2), typeof(Scoped2))]
    [AutoCtor.Scoped(typeof(IScoped3), typeof(Scoped3))]
    private partial class AutoCtorStartupScoped;

    // ------------------------------------------------------------------------

    [BenchmarkCategory(Categories.Transient)]
    [Benchmark]
    public IServiceProvider Jab_Transient() => new JabStartupTransient();

    [BenchmarkCategory(Categories.Transient)]
    [Benchmark]
    public IServiceProvider AutoCtor_Transient() => new AutoCtorStartupTransient();

    [BenchmarkCategory(Categories.Transient)]
    [Benchmark]
    public IServiceProvider MEDI_Transient()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<ITransient1, Transient1>();
        serviceCollection.AddTransient<ITransient2, Transient2>();
        serviceCollection.AddTransient<ITransient3, Transient3>();
        return serviceCollection.BuildServiceProvider();
    }

    [ServiceProvider]
    [Transient(typeof(ITransient1), typeof(Transient1))]
    [Transient(typeof(ITransient2), typeof(Transient2))]
    [Transient(typeof(ITransient3), typeof(Transient3))]
    private partial class JabStartupTransient;

    [AutoCtor.ServiceProvider]
    [AutoCtor.Transient(typeof(ITransient1), typeof(Transient1))]
    [AutoCtor.Transient(typeof(ITransient2), typeof(Transient2))]
    [AutoCtor.Transient(typeof(ITransient3), typeof(Transient3))]
    private partial class AutoCtorStartupTransient;

    // ------------------------------------------------------------------------

    [BenchmarkCategory(Categories.Mixed)]
    [Benchmark]
    public IServiceProvider Jab_Mixed() => new JabStartupMixed();

    [BenchmarkCategory(Categories.Mixed)]
    [Benchmark]
    public IServiceProvider AutoCtor_Mixed() => new AutoCtorStartupMixed();

    [BenchmarkCategory(Categories.Mixed)]
    [Benchmark]
    public IServiceProvider MEDI_Mixed()
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
        return serviceCollection.BuildServiceProvider();
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
    private partial class JabStartupMixed;

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
    private partial class AutoCtorStartupMixed;

    // ------------------------------------------------------------------------

    [BenchmarkCategory(Categories.Complex)]
    [Benchmark]
    public IServiceProvider Jab_Complex() => new JabStartupComplex();

    [BenchmarkCategory(Categories.Complex)]
    [Benchmark]
    public IServiceProvider AutoCtor_Complex() => new AutoCtorStartupComplex();

    [BenchmarkCategory(Categories.Complex)]
    [Benchmark]
    public IServiceProvider MEDI_Complex()
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
        return serviceCollection.BuildServiceProvider();
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
    private partial class JabStartupComplex;

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
    private partial class AutoCtorStartupComplex;
}

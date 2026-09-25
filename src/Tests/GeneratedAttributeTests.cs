using AutoCtor;
using static ExampleTestsHelper;

internal sealed class GeneratedAttributeTests
{
    [Test]
    [ClassDataSource<CompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task AttributeGeneratedCode(CompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Builder
            .WithPreprocessorSymbols(["AUTOCTOR_EMBED_ATTRIBUTES"]);
        var compilation = builder.Build(nameof(GeneratedAttributeTests));
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AttributeSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGenerators(compilation, TestHelper.CancellationToken);

        await Verify(driver)
            .UseMethodName("cs")
            .IgnoreParameters()
            .ConfigureAwait(false);
    }

    [Test]
    [ClassDataSource<CompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task AttributeCompilesProperly(CompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Builder
            .WithPreprocessorSymbols("AUTOCTOR_EMBED_ATTRIBUTES");
        var compilation = builder.Build(nameof(GeneratedAttributeTests));
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AttributeSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics,
                TestHelper.CancellationToken);

        var outputCompilationDiagnostics = outputCompilation
            .GetDiagnostics(TestHelper.CancellationToken);

        await Assert.That(diagnostics).IsEmpty()
            .ConfigureAwait(false);
        await Assert.That(outputCompilationDiagnostics).IsEmpty()
            .ConfigureAwait(false);
    }

    [Test]
    [ClassDataSource<CompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task GenericAttributesCompileProperly(CompilationBuilderFactory builderFactory)
    {
        // The generic [Singleton<..>] attributes are behind their own symbol because generic
        // attributes need C# 11. Without it they are never compiled by any other test.
        var builder = builderFactory.Builder
            .WithPreprocessorSymbols("AUTOCTOR_EMBED_ATTRIBUTES", "AUTOCTOR_EMBED_GENERIC_ATTRIBUTES");
        var compilation = builder.Build(nameof(GeneratedAttributeTests));
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AttributeSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics,
                TestHelper.CancellationToken);

        var outputCompilationDiagnostics = outputCompilation
            .GetDiagnostics(TestHelper.CancellationToken);

        await Assert.That(diagnostics).IsEmpty()
            .ConfigureAwait(false);
        await Assert.That(outputCompilationDiagnostics).IsEmpty()
            .ConfigureAwait(false);
    }

    [Test]
    [ClassDataSource<CompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task ServiceProviderUsagesCompileAgainstEmbeddedAttributes(
        CompilationBuilderFactory builderFactory)
    {
        // Every named argument exists twice: on the package attribute, and on the copy this
        // generator embeds. The other tests here compile the embedded attributes but never
        // use them, so only a real registration catches a property that was added to one and
        // not the other.
        var builder = builderFactory.Builder
            .AddCodes("""
                public interface IZone { }
                public interface IClock { }
                // Unregistered on purpose: without the fallback being read, this is ACTR012.
                public class Clock(IZone zone) : IClock { public IZone Zone => zone; }
                public interface IStamp { }
                public class Stamp : IStamp { }
                public interface IPlugin { }
                public class Plugin : IPlugin { }

                [AutoCtor.ServiceProvider(Fallback = nameof(_host))]
                [AutoCtor.Singleton(typeof(IClock), typeof(Clock), Key = "utc")]
                [AutoCtor.Transient(typeof(IStamp), Factory = nameof(CreateStamp))]
                [AutoCtor.ScanScoped(typeof(IPlugin), As = AutoCtor.ScanAs.Service | AutoCtor.ScanAs.Self, FromAssembliesOf = new[] { typeof(Container) })]
                public partial class Container
                {
                    private readonly System.IServiceProvider _host;
                    public Container(System.IServiceProvider host) => _host = host;
                    private IStamp CreateStamp() => new Stamp();
                }
                """)
            .WithPreprocessorSymbols("AUTOCTOR_EMBED_ATTRIBUTES");
        var compilation = builder.Build(nameof(GeneratedAttributeTests));

        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AttributeSourceGenerator())
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics,
                TestHelper.CancellationToken);

        var outputCompilationDiagnostics = outputCompilation
            .GetDiagnostics(TestHelper.CancellationToken);

        await Assert.That(diagnostics).IsEmpty()
            .ConfigureAwait(false);
        await Assert.That(outputCompilationDiagnostics).IsEmpty()
            .ConfigureAwait(false);
    }

    [Test]
    [ClassDataSource<CompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task PreserveAttributesTest(CompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Builder
            .AddCodes("[AutoCtor.AutoConstruct] public partial class Test { }")
            .WithPreprocessorSymbols("AUTOCTOR_EMBED_ATTRIBUTES", "AUTOCTOR_USAGES");
        var compilation = builder.Build(nameof(GeneratedAttributeTests));

        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AttributeSourceGenerator())
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics,
                TestHelper.CancellationToken);

        var outputCompilationDiagnostics = outputCompilation
            .GetDiagnostics(TestHelper.CancellationToken);

        await Assert.That(diagnostics).IsEmpty()
            .ConfigureAwait(false);
        await Assert.That(outputCompilationDiagnostics).IsEmpty()
            .ConfigureAwait(false);
    }

    [Test]
    [ClassDataSource<CompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task EnsureGeneratedAttributesAreNotExternallyVisible(CompilationBuilderFactory builderFactory)
    {
        // Issue 312
        var compileBuilder = builderFactory.Builder
            .WithPreprocessorSymbols("AUTOCTOR_EMBED_ATTRIBUTES");

        var genDriver = new GeneratorDriverBuilder()
            .AddGenerator(new AttributeSourceGenerator())
            .Build(compileBuilder.ParseOptions);

        var projectA = compileBuilder.Build("ProjectA");

        genDriver = genDriver.RunGeneratorsAndUpdateCompilation(projectA, out var genProjectA, out _, TestHelper.CancellationToken);

        var projectB = compileBuilder
            .AddCompilationReference(genProjectA)
            .Build("ProjectB");

        genDriver.RunGeneratorsAndUpdateCompilation(
            projectB,
            out var outputCompilation,
            out var diagnostics,
            TestHelper.CancellationToken);

        var outputCompilationDiagnostics = outputCompilation
            .GetDiagnostics(TestHelper.CancellationToken);

        await Assert.That(diagnostics).IsEmpty()
            .ConfigureAwait(false);
        await Assert.That(outputCompilationDiagnostics).IsEmpty()
            .ConfigureAwait(false);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance", "CA1812:Avoid uninstantiated internal classes",
        Justification = "Instantiated in generated code.")]
    internal sealed class CompilationBuilderFactory : ExampleTestsHelper.CompilationBuilderFactory;
}

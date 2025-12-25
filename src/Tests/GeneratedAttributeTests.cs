using AutoCtor;
using static ExampleTestsHelper;

public class GeneratedAttributeTests
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

    public class CompilationBuilderFactory : CompilationBuilderFactoryBase;
}

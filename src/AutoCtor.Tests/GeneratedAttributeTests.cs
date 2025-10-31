namespace AutoCtor.Tests;

public class GeneratedAttributeTests
{
    [Test]
    public async Task AttributeGeneratedCode()
    {
        var builder = new CompilationBuilder()
            .AddNetCoreReference()
            .WithPreprocessorSymbols(["AUTOCTOR_EMBED_ATTRIBUTES"]);
        var compilation = await builder.Build(nameof(GeneratedAttributeTests), TestHelper.CancellationToken)
            .ConfigureAwait(false);
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AttributeSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGenerators(compilation, TestHelper.CancellationToken);

        await Verify(driver)
            .ConfigureAwait(false);
    }

    [Test]
    public async Task AttributeCompilesProperly()
    {
        var builder = new CompilationBuilder()
            .AddNetCoreReference()
            .WithPreprocessorSymbols(["AUTOCTOR_EMBED_ATTRIBUTES"]);
        var compilation = await builder.Build(nameof(GeneratedAttributeTests), TestHelper.CancellationToken)
            .ConfigureAwait(false);
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
    public async Task PreserveAttributesTest()
    {
        var builder = new CompilationBuilder()
            .AddNetCoreReference()
            .AddCode("[AutoCtor.AutoConstruct] public partial class Test { }")
            .WithPreprocessorSymbols(["AUTOCTOR_EMBED_ATTRIBUTES", "AUTOCTOR_USAGES"]);
        var compilation = await builder.Build(nameof(GeneratedAttributeTests), TestHelper.CancellationToken)
            .ConfigureAwait(false);

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
    public async Task EnsureGeneratedAttributesAreNotExternallyVisible()
    {
        // Issue 312
        var compileBuilder = new CompilationBuilder()
            .AddNetCoreReference()
            .WithPreprocessorSymbols(["AUTOCTOR_EMBED_ATTRIBUTES"]);

        var genDriver = new GeneratorDriverBuilder()
            .AddGenerator(new AttributeSourceGenerator())
            .Build(compileBuilder.ParseOptions);

        var projectA = await compileBuilder
            .Build("ProjectA", TestHelper.CancellationToken)
            .ConfigureAwait(false);

        genDriver = genDriver.RunGeneratorsAndUpdateCompilation(projectA, out var genProjectA, out _, TestHelper.CancellationToken);

        var projectB = await compileBuilder
            .AddCompilationReference(genProjectA)
            .Build("ProjectB", TestHelper.CancellationToken)
            .ConfigureAwait(false);

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
}

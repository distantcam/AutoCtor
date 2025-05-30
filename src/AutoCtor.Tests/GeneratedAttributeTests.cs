namespace AutoCtor.Tests;

public class GeneratedAttributeTests
{
    [Fact]
    public async Task AttributeGeneratedCode()
    {
        var builder = new CompilationBuilder()
            .AddNetCoreReference()
            .WithPreprocessorSymbols(["AUTOCTOR_EMBED_ATTRIBUTES"]);
        var compilation = await builder.Build(nameof(GeneratedAttributeTests));
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AttributeSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGenerators(compilation, TestContext.Current.CancellationToken);

        await Verify(driver);
    }

    [Fact]
    public async Task AttributeCompilesProperly()
    {
        var builder = new CompilationBuilder()
            .AddNetCoreReference()
            .WithPreprocessorSymbols(["AUTOCTOR_EMBED_ATTRIBUTES"]);
        var compilation = await builder.Build(nameof(GeneratedAttributeTests));
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AttributeSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics,
                TestContext.Current.CancellationToken);

        Assert.Empty(diagnostics);
        Assert.Empty(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task PreserveAttributesTest()
    {
        var builder = new CompilationBuilder()
            .AddNetCoreReference()
            .AddCode("[AutoCtor.AutoConstruct] public partial class Test { }")
            .WithPreprocessorSymbols(["AUTOCTOR_EMBED_ATTRIBUTES", "AUTOCTOR_USAGES"]);
        var compilation = await builder.Build(nameof(GeneratedAttributeTests));

        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AttributeSourceGenerator())
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics,
                TestContext.Current.CancellationToken);

        Assert.Empty(diagnostics);
        Assert.Empty(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
    }

    [Fact]
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
            .Build("ProjectA");

        genDriver = genDriver.RunGeneratorsAndUpdateCompilation(projectA, out var genProjectA, out _, TestContext.Current.CancellationToken);

        var projectB = await compileBuilder
            .AddCompilationReference(genProjectA)
            .Build("ProjectB");

        genDriver.RunGeneratorsAndUpdateCompilation(
            projectB,
            out var outputCompilation,
            out var diagnostics,
            TestContext.Current.CancellationToken);

        Assert.Empty(diagnostics);
        Assert.Empty(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
    }
}

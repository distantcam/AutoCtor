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
            .RunGenerators(compilation);

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
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.Empty(diagnostics);
        Assert.Empty(outputCompilation.GetDiagnostics());
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
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.Empty(diagnostics);
        Assert.Empty(outputCompilation.GetDiagnostics());
    }
}

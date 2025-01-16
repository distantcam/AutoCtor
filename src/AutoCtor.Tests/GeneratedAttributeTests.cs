using Microsoft.CodeAnalysis;

namespace AutoCtor.Tests;

public class GeneratedAttributeTests
{
    [Fact]
    public async Task AttributeGeneratedCode()
    {
        var compilation = await Helpers.Compile<AutoConstructAttribute>([], preprocessorSymbols: ["AUTOCTOR_EMBED_ATTRIBUTES"]);
        var generator = new AttributeSourceGenerator().AsSourceGenerator();
        var driver = Helpers.CreateDriver(generator)
            .RunGenerators(compilation, TestContext.Current.CancellationToken);

        await Verify(driver).UseDirectory("Verified");
    }

    [Fact]
    public async Task AttributeCompilesProperly()
    {
        var compilation = await Helpers.Compile<AutoConstructAttribute>([], preprocessorSymbols: ["AUTOCTOR_EMBED_ATTRIBUTES"]);
        var generator = new AttributeSourceGenerator().AsSourceGenerator();
        Helpers.CreateDriver(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics, TestContext.Current.CancellationToken);

        Assert.Empty(diagnostics);
        Assert.Empty(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
    }
}

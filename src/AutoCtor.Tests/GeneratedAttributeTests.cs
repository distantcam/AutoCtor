using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoCtor.Tests;

public class GeneratedAttributeTests
{
    [Fact]
    public void CompileGeneratedAttributes()
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create(
            assemblyName: "AttributeProjectTest",
            syntaxTrees: null,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new AutoConstructSourceGenerator();
        var parseOptions = new CSharpParseOptions(preprocessorSymbols: ["AUTOCTOR_EMBED_ATTRIBUTES"]);
#if ROSLYN_3_11
        var driver = CSharpGeneratorDriver.Create([generator], parseOptions: parseOptions);
#elif ROSLYN_4_0 || ROSLYN_4_4
        var driver = CSharpGeneratorDriver.Create(generator).WithUpdatedParseOptions(parseOptions);
#endif
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics()
            .Should().BeEmpty();
    }
}

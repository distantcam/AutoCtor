using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoCtor.Tests;

[UsesVerify]
public class Issue73
{
    private readonly VerifySettings _codeVerifySettings;

    public Issue73()
    {
        _codeVerifySettings = new();
        _codeVerifySettings.ScrubLinesContaining("Version:", "SHA:");
    }

    [Fact]
    public Task VerifyGeneratedCode()
    {
        var compilation = Compile();

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver, _codeVerifySettings)
            .UseDirectory("Verified");
    }

    [Fact]
    public void CodeCompilesWithoutErrors()
    {
        var ignoredWarnings = new string[] {
            "CS0414" // Ignore unused fields
        };

        var compilation = Compile();

        var generator = new AutoConstructSourceGenerator();
        CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics()
            .Where(d => !ignoredWarnings.Contains(d.Id))
            .Should().BeEmpty();
    }

    private CSharpCompilation Compile()
    {
        var projectACode = @"
using AutoCtor;
namespace A;
public interface Interface<T, U>{}
";
        var projectBCode = @"
using AutoCtor;
using A;
namespace B;
public abstract class BaseClass<T, U, V>
{
    private readonly Interface<T, U> _interface;
    public BaseClass(Interface<T, U> i) => _interface = i;
}
[AutoConstruct]
public sealed partial class TheClass : BaseClass<object, int, string>{}
"
        ;

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        var projectA = CSharpCompilation.Create(
            assemblyName: "ProjectA",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(projectACode) },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var projectB = CSharpCompilation.Create(
            assemblyName: "ProjectB",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(projectBCode) },
            references: references.Concat(new[] { projectA.ToMetadataReference() }),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return projectB;
    }
}

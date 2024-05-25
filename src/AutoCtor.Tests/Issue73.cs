using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoCtor.Tests;

public class Issue73
{
    [Fact]
    public async Task VerifyGeneratedCode()
    {
        var compilation = await Compile();
        var generator = new AutoConstructSourceGenerator().AsSourceGenerator();
        var driver = Helpers.CreateDriver([], generator).RunGenerators(compilation);

        await Verify(driver).UseDirectory("Verified");
    }

    [Fact]
    public async Task CodeCompilesWithoutErrors()
    {
        string[] ignoredWarnings = ["CS0414"]; // Ignore unused fields

        var compilation = await Compile();
        var generator = new AutoConstructSourceGenerator().AsSourceGenerator();
        Helpers.CreateDriver([], generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics()
            .Where(d => !ignoredWarnings.Contains(d.Id))
            .Should().BeEmpty();
    }

    private static async Task<CSharpCompilation> Compile()
    {
        var projectACode = @"
namespace A
{
public interface Interface<T, U>{}
}
";
        var projectBCode = @"
using AutoCtor;
using A;
namespace B
{
public abstract class BaseClass<T, U, V>
{
    private readonly Interface<T, U> _interface;
    public BaseClass(Interface<T, U> i) => _interface = i;
}
[AutoConstruct]
public sealed partial class TheClass : BaseClass<object, int, string>{}
}
";

        var projectA = await Helpers.Compile([projectACode], "ProjectA");
        var projectB = await Helpers.Compile([projectBCode], "ProjectB",
            extraReferences: [projectA.ToMetadataReference()]);

        return projectB;
    }
}

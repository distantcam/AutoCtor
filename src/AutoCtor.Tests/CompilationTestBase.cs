using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoCtor.Tests;

public abstract class CompilationTestBase
{
    private readonly VerifySettings _codeVerifySettings;
    public CompilationTestBase()
    {
        _codeVerifySettings = new();
        _codeVerifySettings.ScrubLinesContaining("Version:", "SHA:", "GeneratedCodeAttribute");
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

    protected abstract CSharpCompilation Compile();
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoCtor.Tests;

public class Issue73
{
    [Fact]
    public async Task VerifyGeneratedCode()
    {
        var common = Common();
        var compilation = await Compile(common);
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(common.ParseOptions)
            .RunGenerators(compilation, TestContext.Current.CancellationToken);

        await Verify(driver);
    }

    [Fact]
    public async Task CodeCompilesWithoutErrors()
    {
        string[] ignoredWarnings = ["CS0414"]; // Ignore unused fields

        var common = Common();
        var compilation = await Compile(common);
        new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(common.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics,
                TestContext.Current.CancellationToken);

        Assert.Empty(diagnostics);
        Assert.Empty(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(d => !ignoredWarnings.Contains(d.Id)));
    }

    private static CompilationBuilder Common()
    {
        return new CompilationBuilder()
            .AddNetCoreReference()
            .AddAssemblyReference<AutoConstructAttribute>();
    }

    private static async Task<CSharpCompilation> Compile(CompilationBuilder common)
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
        var projectA = await common
            .AddCode(projectACode)
            .Build("ProjectA");

        var projectB = await common
            .AddCompilationReference(projectA)
            .AddCode(projectBCode)
            .Build("ProjectB");

        return projectB;
    }
}

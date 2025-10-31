using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoCtor.Tests;

public class Issue73
{
    [Test]
    public async Task VerifyGeneratedCode()
    {
        var common = Common();
        var compilation = await Compile(common)
            .ConfigureAwait(false);
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(common.ParseOptions)
            .RunGenerators(compilation, TestHelper.CancellationToken);

        await Verify(driver)
            .ConfigureAwait(false);
    }

    [Test]
    public async Task CodeCompilesWithoutErrors()
    {
        string[] ignoredWarnings = ["CS0414"]; // Ignore unused fields

        var common = Common();
        var compilation = await Compile(common)
            .ConfigureAwait(false);
        new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(common.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics,
                TestHelper.CancellationToken);

        var outputCompilationDiagnostics = outputCompilation
            .GetDiagnostics(TestHelper.CancellationToken)
            .Where(d => !ignoredWarnings.Contains(d.Id));

        await Assert.That(diagnostics).IsEmpty();
        await Assert.That(outputCompilationDiagnostics).IsEmpty();
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
            .Build("ProjectA", TestHelper.CancellationToken)
            .ConfigureAwait(false);

        var projectB = await common
            .AddCompilationReference(projectA)
            .AddCode(projectBCode)
            .Build("ProjectB", TestHelper.CancellationToken)
            .ConfigureAwait(false);

        return projectB;
    }
}

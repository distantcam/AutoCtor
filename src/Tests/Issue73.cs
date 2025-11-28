using AutoCtor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static ExampleTestsHelper;

public class Issue73
{
    [Test]
    [ClassDataSource<CompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task VerifyGeneratedCode(CompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Builder;
        var compilation = Compile(builder);
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGenerators(compilation, TestHelper.CancellationToken);

        await Verify(driver)
            .UseMethodName("cs")
            .IgnoreParameters()
            .ConfigureAwait(false);
    }

    [Test]
    [ClassDataSource<CompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task CodeCompilesWithoutErrors(CompilationBuilderFactory builderFactory)
    {
        string[] ignoredWarnings = ["CS0414"]; // Ignore unused fields

        var builder = builderFactory.Builder;
        var compilation = Compile(builder);
        new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out var diagnostics,
                TestHelper.CancellationToken);

        var outputCompilationDiagnostics = outputCompilation
            .GetDiagnostics(TestHelper.CancellationToken)
            .Where(d => !ignoredWarnings.Contains(d.Id));

        await Assert.That(diagnostics).IsEmpty()
            .ConfigureAwait(false);
        await Assert.That(outputCompilationDiagnostics).IsEmpty()
            .ConfigureAwait(false);
    }

    private static CSharpCompilation Compile(CompilationBuilder common)
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
        var projectA = common
            .AddCode(projectACode)
            .Build("ProjectA");

        var projectB = common
            .AddCompilationReference(projectA)
            .AddCode(projectBCode)
            .Build("ProjectB");

        return projectB;
    }

    public class CompilationBuilderFactory : CompilationBuilderFactoryBase<AutoConstructAttribute>;
}

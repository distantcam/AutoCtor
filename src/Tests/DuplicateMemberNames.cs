using AutoCtor;
using static ExampleTestsHelper;

internal sealed class DuplicateMemberNames
{
    // Code mid-edit can declare the same member name twice (CS0102). The generator has to
    // keep producing output instead of throwing, which drops every generated constructor.
    [Test]
    [ClassDataSource<CompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task GeneratorDoesNotThrow(CompilationBuilderFactory builderFactory)
    {
        const string code = @"
public interface IService { }
public interface IOther { }
public interface IAnother { }

[AutoCtor.AutoConstruct]
public partial class DuplicateMembers
{
    private readonly IService _service;
    private readonly IService _service;
    private readonly IOther _other;
    private readonly IAnother _other;
}
";
        var builder = builderFactory.Builder;
        var compilation = builder
            .AddCodes(code)
            .Build(nameof(DuplicateMemberNames));

        var result = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGenerators(compilation, TestHelper.CancellationToken)
            .GetRunResult()
            .Results.Single();

        await Assert.That(result.Exception).IsNull()
            .ConfigureAwait(false);
        await Assert.That(result.GeneratedSources).IsNotEmpty()
            .ConfigureAwait(false);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance", "CA1812:Avoid uninstantiated internal classes",
        Justification = "Instantiated in generated code.")]
    internal sealed class CompilationBuilderFactory : CompilationBuilderFactory<AutoConstructAttribute>;
}

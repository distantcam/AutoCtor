#if ROSLYN_4_4
using AutoCtor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static ExampleTests;

internal sealed class IncrementalOutputTests
{
    [Test]
    [ClassDataSource<BareCompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task EditingATypeRegeneratesOnlyItAndItsDerivedTypes(BareCompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Builder;
        const string Other = "[AutoCtor.AutoConstruct] public partial class Other { private readonly int _o; }";
        const string Derived = "[AutoCtor.AutoConstruct] public partial class Derived : Base { private readonly int _d; }";
        var app = builder.AddCodes(
            "[AutoCtor.AutoConstruct] public partial class Base { private readonly int _b; }", Other, Derived);

        var compilation = app.Build("App");
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(app.ParseOptions)
            .RunGenerators(compilation, TestHelper.CancellationToken);

        // Replace only the edited tree, as the IDE does, so the other files keep their trees.
        var baseTree = compilation.SyntaxTrees.Single(t => t.ToString().Contains("class Base", StringComparison.Ordinal));
        var edited = compilation.ReplaceSyntaxTree(baseTree, CSharpSyntaxTree.ParseText(
            "[AutoCtor.AutoConstruct] public partial class Base { private readonly int _b, _added; }",
            (CSharpParseOptions)baseTree.Options, cancellationToken: TestHelper.CancellationToken));
        var result = driver.RunGenerators(edited, TestHelper.CancellationToken)
            .GetRunResult().Results[0];

        var reasons = result.TrackedOutputSteps["SourceOutput"]
            .SelectMany(s => s.Outputs)
            .Select(o => o.Reason)
            .ToList();
        await Assert.That(reasons).Contains(IncrementalStepRunReason.Cached).ConfigureAwait(false);
        await Assert.That(reasons).Contains(IncrementalStepRunReason.Modified).ConfigureAwait(false);

        // The derived type picks up its base's new parameter.
        await Assert.That(result.GeneratedSources
            .Single(s => s.HintName == "Derived.g.cs").SourceText.ToString())
            .Contains("added")
            .ConfigureAwait(false);
    }
}
#endif

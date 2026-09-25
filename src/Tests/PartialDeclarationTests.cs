using AutoCtor;
using Microsoft.CodeAnalysis;
using static ExampleTests;

internal sealed class PartialDeclarationTests
{
    // Every generator version visits each declaration, so a type declared in several files with
    // attributes on more than one of them must still be generated once.
    [Test]
    [ClassDataSource<BareCompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task PartialTypesAreGeneratedOnce(BareCompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Builder.AddCodes(
            """
            public partial class Service;
            [AutoCtor.ServiceProvider]
            public partial class Container;
            """,
            """
            [AutoCtor.AutoConstruct]
            public partial class Service { private readonly IClock _clock; }
            [AutoCtor.Singleton(typeof(IClock), typeof(Clock))]
            public partial class Container;
            """,
            """
            [System.Serializable]
            public partial class Service;
            [AutoCtor.Singleton(typeof(Service))]
            public partial class Container;
            public interface IClock;
            public class Clock : IClock;
            """);

        new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(builder.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                builder.Build(nameof(PartialDeclarationTests)),
                out var outputCompilation,
                out var diagnostics,
                TestHelper.CancellationToken);

        await Assert.That(diagnostics.Concat(outputCompilation.GetDiagnostics(TestHelper.CancellationToken))
            .Where(d => d.Severity == DiagnosticSeverity.Error))
            .IsEmpty()
            .ConfigureAwait(false);
        await Assert.That(outputCompilation.SyntaxTrees.Count(t => t.FilePath.EndsWith("Container.ServiceProvider.g.cs", StringComparison.Ordinal)))
            .IsEqualTo(1)
            .ConfigureAwait(false);
    }
}

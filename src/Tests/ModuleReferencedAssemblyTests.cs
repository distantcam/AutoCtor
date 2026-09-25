using AutoCtor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static ExampleTests;
using static ExampleTestsHelper;

internal sealed class ModuleReferencedAssemblyTests
{
    private const string Library = """
        [AutoCtor.Singleton(typeof(ILibraryService), typeof(LibraryService))]
        [AutoCtor.Singleton(typeof(ILibraryClock), Factory = nameof(CreateClock))]
        public class LibraryModule
        {
            public static ILibraryClock CreateClock() => new LibraryClock();
        }
        public interface ILibraryService;
        public class LibraryService(ILibraryClock clock) : ILibraryService
        {
            public ILibraryClock Clock => clock;
        }
        public interface ILibraryClock;
        internal class LibraryClock : ILibraryClock;
        """;

    private const string App = """
        [AutoCtor.ServiceProvider]
        [AutoCtor.Import(typeof(LibraryModule))]
        public partial class Container;
        """;

    [Test]
    [ClassDataSource<BareCompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task ImportsModuleFromReferencedAssembly(BareCompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Builder;
        // Without the symbol the registration attributes are not compiled into the library.
        var library = await EmitReference(builder
            .WithPreprocessorSymbols([.. PreprocessorSymbols, "AUTOCTOR_USAGES"])
            .AddCodes(Library).Build("Library")).ConfigureAwait(false);
        var app = builder.AddReferences(library).AddCodes(App);

        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(app.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                app.Build("App"),
                out var outputCompilation,
                out _,
                TestHelper.CancellationToken);

        // Each compilation carries its own copy of the test polyfills, which only warns.
        await Assert.That(outputCompilation.GetDiagnostics(TestHelper.CancellationToken)
            .Where(d => d.Severity == DiagnosticSeverity.Error))
            .IsEmpty()
            .ConfigureAwait(false);

        await Verify(driver)
            .IgnoreParameters()
            .ConfigureAwait(false);
    }

    [Test]
    [ClassDataSource<BareCompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task ModuleBuiltWithoutUsagesIsReported(BareCompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Builder;
        var library = await EmitReference(builder.AddCodes(Library).Build("Library")).ConfigureAwait(false);
        var app = builder.AddReferences(library).AddCodes(App);

        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(app.ParseOptions)
            .RunGenerators(app.Build("App"), TestHelper.CancellationToken);

        await Assert.That(driver.GetRunResult().Diagnostics.Select(d => d.Id))
            .Contains("ACTR027")
            .ConfigureAwait(false);
    }

    // A compilation reference hands over source symbols, which keep [Conditional] attributes
    // whatever the symbols say. Only emitted metadata shows what a real library carries.
    private static async Task<MetadataReference> EmitReference(CSharpCompilation library)
    {
        using var stream = new MemoryStream();
        var result = library.Emit(stream, cancellationToken: TestHelper.CancellationToken);
        await Assert.That(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
            .IsEmpty()
            .ConfigureAwait(false);
        return MetadataReference.CreateFromImage(stream.ToArray());
    }

#if ROSLYN_4_4
    [Test]
    [ClassDataSource<BareCompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task EditingAModuleRegeneratesTheProvider(BareCompilationBuilderFactory builderFactory)
    {
        const string provider = """
            [AutoCtor.ServiceProvider]
            [AutoCtor.Import(typeof(LocalModule))]
            public partial class Container;
            """;
        const string module = """
            [AutoCtor.Singleton(typeof(LocalService))]
            public class LocalModule;
            public class LocalService;
            """;

        var app = builderFactory.Builder.AddCodes(provider, module);
        var compilation = app.Build("App");

        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(app.ParseOptions)
            .RunGenerators(compilation, TestHelper.CancellationToken);

        // Only the module's file changes; the provider's syntax tree is the same one.
        var moduleTree = compilation.SyntaxTrees.Single(t => t.ToString().Contains("class LocalModule", StringComparison.Ordinal));
        var editedTree = CSharpSyntaxTree.ParseText(
            "[AutoCtor.Singleton(typeof(AddedService))]\n" + module + "\npublic class AddedService;",
            app.ParseOptions,
            cancellationToken: TestHelper.CancellationToken);
        driver = driver.RunGenerators(compilation.ReplaceSyntaxTree(moduleTree, editedTree), TestHelper.CancellationToken);

        await Assert.That(driver.GetRunResult().GeneratedTrees
            .Any(t => t.ToString().Contains("global::AddedService", StringComparison.Ordinal)))
            .IsTrue()
            .ConfigureAwait(false);
    }
#endif
}

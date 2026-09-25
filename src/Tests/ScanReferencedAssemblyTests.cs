using AutoCtor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static ExampleTests;

internal sealed class ScanReferencedAssemblyTests
{
    private const string PluginLibrary = """
        public interface IPlugin;
        public class PluginMarker;
        public class PublicPlugin : IPlugin;
        internal class InternalPlugin : IPlugin;
        """;

    // Grants its internals to the app, so its internal plugin is found and the other
    // library's is not.
    private const string FriendLibrary = """
        [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("App")]
        public class FriendMarker;
        public class FriendPlugin : IPlugin;
        internal class InternalFriendPlugin : IPlugin;
        """;

    private const string App = """
        [AutoCtor.ServiceProvider]
        [AutoCtor.ScanSingleton(typeof(IPlugin),
            FromAssembliesOf = new[] { typeof(Container), typeof(PluginMarker), typeof(FriendMarker) })]
        public partial class Container;
        public class LocalPlugin : IPlugin;
        """;

    [Test]
    [ClassDataSource<BareCompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task ScansReferencedAssemblies(BareCompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Builder;
        var pluginLibrary = builder.AddCodes(PluginLibrary).Build("PluginLibrary");
        var friendLibrary = builder.AddCompilationReference(pluginLibrary)
            .AddCodes(FriendLibrary).Build("FriendLibrary");
        var app = builder.AddCompilationReference(pluginLibrary)
            .AddCompilationReference(friendLibrary)
            .AddCodes(App);

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

#if ROSLYN_4_4
    [Test]
    [ClassDataSource<BareCompilationBuilderFactory>(Shared = SharedType.PerTestSession)]
    public async Task NewMatchingTypesArePickedUp(BareCompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Builder;
        var pluginLibrary = builder.AddCodes(PluginLibrary).Build("PluginLibrary");
        var friendLibrary = builder.AddCompilationReference(pluginLibrary)
            .AddCodes(FriendLibrary).Build("FriendLibrary");
        var app = builder.AddCompilationReference(pluginLibrary)
            .AddCompilationReference(friendLibrary)
            .AddCodes(App);

        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .Build(app.ParseOptions)
            .RunGenerators(app.Build("App"), TestHelper.CancellationToken);

        // A new type in the provider's own assembly.
        var addedLocally = app.AddCodes("public class AddedPlugin : IPlugin;");
        driver = driver.RunGenerators(addedLocally.Build("App"), TestHelper.CancellationToken);
        await AssertPickedUp(driver, "AddedPlugin").ConfigureAwait(false);

        // A new type in a referenced assembly, which reaches the app as a changed reference.
        var changedLibrary = pluginLibrary.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
            "public class AddedLibraryPlugin : IPlugin;", app.ParseOptions,
            cancellationToken: TestHelper.CancellationToken));
        var addedInLibrary = builder.AddCompilationReference(changedLibrary)
            .AddCompilationReference(friendLibrary)
            .AddCodes(App, "public class AddedPlugin : IPlugin;");
        driver = driver.RunGenerators(addedInLibrary.Build("App"), TestHelper.CancellationToken);
        await AssertPickedUp(driver, "AddedLibraryPlugin").ConfigureAwait(false);
    }

    private static async Task AssertPickedUp(GeneratorDriver driver, string typeName)
    {
        var result = driver.GetRunResult().Results[0];

        await Assert.That(result.TrackedSteps[AutoConstructSourceGenerator.TrackingNames.ServiceProviders]
            .SelectMany(s => s.Outputs)
            .Select(o => o.Reason))
            .Contains(IncrementalStepRunReason.Modified)
            .ConfigureAwait(false);

        await Assert.That(result.GeneratedSources
            .Any(s => s.SourceText.ToString().Contains($"global::{typeName}", StringComparison.Ordinal)))
            .IsTrue()
            .ConfigureAwait(false);
    }
#endif
}

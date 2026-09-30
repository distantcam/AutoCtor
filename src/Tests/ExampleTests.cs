using AutoCtor;
using Microsoft.CodeAnalysis;
using static ExampleTestsHelper;
using Microsoft.CodeAnalysis.Testing;


#if ROSLYN_4_4
using Microsoft.CodeAnalysis.CSharp;
#endif

internal sealed class ExampleTests
{
    [Test]
    [CombinedDataSources]
    public Task ExamplesGeneratedCode(
        [MethodDataSource(nameof(GetExamples))] CodeFileTheoryData theoryData,
        [ClassDataSource<CompilationBuilderFactory>(Shared = SharedType.PerTestSession)] CompilationBuilderFactory builderFactory)
        => VerifyGeneratedCode(theoryData, builderFactory);

    [Test]
    [CombinedDataSources]
    public Task CodeCompilesProperly(
        [MethodDataSource(nameof(GetExamples))] CodeFileTheoryData theoryData,
        [ClassDataSource<CompilationBuilderFactory>(Shared = SharedType.PerTestSession)] CompilationBuilderFactory builderFactory)
        => AssertCompiles(theoryData, builderFactory);

    // Without Microsoft.Extensions.DependencyInjection.Abstractions.
    [Test]
    [CombinedDataSources]
    public Task IoCGeneratedCode(
        [MethodDataSource(nameof(GetIoCExamples))] CodeFileTheoryData theoryData,
        [ClassDataSource<BareCompilationBuilderFactory>(Shared = SharedType.PerTestSession)] BareCompilationBuilderFactory builderFactory)
        => VerifyGeneratedCode(theoryData, builderFactory);

    [Test]
    [CombinedDataSources]
    public Task IoCCodeCompilesProperly(
        [MethodDataSource(nameof(GetIoCExamples))] CodeFileTheoryData theoryData,
        [ClassDataSource<BareCompilationBuilderFactory>(Shared = SharedType.PerTestSession)] BareCompilationBuilderFactory builderFactory)
        => AssertCompiles(theoryData, builderFactory);

    // IAsyncDisposable isn't in netstandard2.0.
    [Test]
    [CombinedDataSources]
    public Task AsyncGeneratedCode(
        [MethodDataSource(nameof(GetAsyncExamples))] CodeFileTheoryData theoryData,
        [ClassDataSource<AsyncCompilationBuilderFactory>(Shared = SharedType.PerTestSession)] AsyncCompilationBuilderFactory builderFactory)
        => VerifyGeneratedCode(theoryData, builderFactory);

    [Test]
    [CombinedDataSources]
    public Task AsyncCodeCompilesProperly(
        [MethodDataSource(nameof(GetAsyncExamples))] CodeFileTheoryData theoryData,
        [ClassDataSource<AsyncCompilationBuilderFactory>(Shared = SharedType.PerTestSession)] AsyncCompilationBuilderFactory builderFactory)
        => AssertCompiles(theoryData, builderFactory);

    private static async Task VerifyGeneratedCode(CodeFileTheoryData theoryData, ExampleTestsHelper.CompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Create(theoryData);
        var compilation = builder.Build(nameof(ExampleTests));
        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .WithAnalyzerOptions(theoryData.Options)
            .Build(builder.ParseOptions)
            .RunGenerators(compilation, TestHelper.CancellationToken);

        await Verify(driver)
            .UseDirectory(theoryData.VerifiedDirectory)
            .UseTypeName(theoryData.Name)
            .UseMethodName("cs")
            .IgnoreParameters()
            .ConfigureAwait(false);
    }

    private static async Task AssertCompiles(CodeFileTheoryData theoryData, ExampleTestsHelper.CompilationBuilderFactory builderFactory)
    {
        if (theoryData.SnapshotOnly)
            return;

        var builder = builderFactory.Create(theoryData);
        var compilation = builder.Build(nameof(ExampleTests));
        new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .WithAnalyzerOptions(theoryData.Options)
            .Build(builder.ParseOptions)
            .RunGeneratorsAndUpdateCompilation(
                compilation,
                out var outputCompilation,
                out _,
                TestHelper.CancellationToken);

        await Assert.That(outputCompilation.GetDiagnostics(TestHelper.CancellationToken)
            .Where(d => !theoryData.IgnoredCompileDiagnostics.Contains(d.Id)))
            .IsEmpty()
            .ConfigureAwait(false);
    }

#if ROSLYN_4_4
    [Test]
    [CombinedDataSources]
    public async Task EnsureRunsAreCachedCorrectly(
        [MethodDataSource(nameof(GetExamples))] CodeFileTheoryData theoryData,
        [ClassDataSource<CompilationBuilderFactory>(Shared = SharedType.PerTestSession)] CompilationBuilderFactory builderFactory)
    {
        var builder = builderFactory.Create(theoryData);
        var compilation = builder.Build(nameof(ExampleTests));

        var driver = new GeneratorDriverBuilder()
            .AddGenerator(new AutoConstructSourceGenerator())
            .WithAnalyzerOptions(theoryData.Options)
            .Build(builder.ParseOptions);

        driver = driver.RunGenerators(compilation, TestHelper.CancellationToken);
        var firstResult = driver.GetRunResult();

        // Change the compilation
        compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("// dummy",
            CSharpParseOptions.Default.WithLanguageVersion(theoryData.LangPreview
                ? LanguageVersion.Preview
                : LanguageVersion.Latest),
            cancellationToken: TestHelper.CancellationToken));

        driver = driver.RunGenerators(compilation, TestHelper.CancellationToken);
        var secondResult = driver.GetRunResult();

        await AssertRunsEqual(firstResult, secondResult,
            AutoConstructSourceGenerator.TrackingNames.AllTrackers)
            .ConfigureAwait(false);
    }
#endif

    // ----------------------------------------------------------------------------------------

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance", "CA1812:Avoid uninstantiated internal classes",
        Justification = "Instantiated in generated code.")]
    internal sealed class CompilationBuilderFactory : CompilationBuilderFactory<AutoConstructAttribute>
    {
        protected override IEnumerable<string> GetNuGetIds() => ["Microsoft.Extensions.DependencyInjection.Abstractions"];
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance", "CA1812:Avoid uninstantiated internal classes",
        Justification = "Instantiated in generated code.")]
    internal sealed class BareCompilationBuilderFactory : CompilationBuilderFactory<AutoConstructAttribute>;

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance", "CA1812:Avoid uninstantiated internal classes",
        Justification = "Instantiated in generated code.")]
    internal sealed class AsyncCompilationBuilderFactory : CompilationBuilderFactory<AutoConstructAttribute>
    {
        protected override ReferenceAssemblies BaseReferenceAssemblies => ReferenceAssemblies.Net.Net100;
        protected override IEnumerable<string> GetNuGetIds() => ["Microsoft.Extensions.DependencyInjection.Abstractions"];
    }

    private static IEnumerable<Func<CodeFileTheoryData>> GetIoCExamples(string examples, string diagnosticExamples) =>
        GetExamplesFiles(examples).Select<string, Func<CodeFileTheoryData>>(example => () => new CodeFileTheoryData(example))
        .Concat(GetExamplesFiles(diagnosticExamples).Select<string, Func<CodeFileTheoryData>>(example => () => new CodeFileTheoryData(example) { SnapshotOnly = true }));

    public static IEnumerable<Func<CodeFileTheoryData>> GetIoCExamples() => GetIoCExamples("IoCExamples", "IoCDiagnosticExamples");

    public static IEnumerable<Func<CodeFileTheoryData>> GetAsyncExamples() => GetIoCExamples("IoCAsyncExamples", "IoCAsyncDiagnosticExamples");

    public static IEnumerable<Func<CodeFileTheoryData>> GetExamples()
    {
        foreach (var example in GetExamplesFiles("Examples"))
        {
            yield return () => new CodeFileTheoryData(example) with
            {
                IgnoredCompileDiagnostics = ["CS0414", "CS0169"] // Ignore unused fields
            };
        }

        foreach (var guardExample in GetExamplesFiles("GuardExamples"))
        {
            yield return () => new CodeFileTheoryData(guardExample) with
            {
                Options = new() { { "build_property.AutoCtorGuards", "true" } }
            };
        }

        foreach (var langExample in GetExamplesFiles("LangExamples"))
        {
            var verifiedName = string.Concat("Verified_", PreprocessorSymbols.Last().AsSpan(7));
            yield return () => new CodeFileTheoryData(langExample) with
            {
                VerifiedDirectory = Path.Combine(Path.GetDirectoryName(langExample) ?? "", verifiedName),
                LangPreview = true,
            };
        }

        foreach (var example in GetIoCExamples("IoCAbstractionsExamples", "IoCAbstractionsDiagnosticExamples"))
            yield return example;

#if ROSLYN_4_4
        foreach (var readmeExample in GetExamplesFiles("ReadmeExamples"))
        {
            yield return () => new CodeFileTheoryData(readmeExample) with
            {
                IgnoredCompileDiagnostics = ["CS0414", "CS0169"] // Ignore unused fields
            };
        }
#endif
    }
}

using AutoCtor;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace CompileBenchmarks;

[Config(typeof(DefaultConfig))]
[MemoryDiagnoser]
public class Scanner
{
    [Params(1000, 5000)]
    public int PluginCount { get; set; }

    private CSharpCompilation _compilation = null!;
    private GeneratorDriver _driver = null!;
    private GeneratorDriver _warmDriver = null!;
    private CSharpCompilation _editedCompilation = null!;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        var frameworkReferences = await ReferenceAssemblies.Net.Net100
            .ResolveAsync(CSharpParseOptions.Default.Language, CancellationToken.None)
            .ConfigureAwait(false);

        _compilation = CSharpCompilation.Create(nameof(Scanner))
            .WithOptions(new(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(frameworkReferences)
            .AddReferences(MetadataReference
                .CreateFromFile(typeof(AutoConstructAttribute).Assembly.Location))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText("public interface IPlugin;"))
            .AddSyntaxTrees(Enumerable.Range(1, PluginCount)
                .Select(i => CSharpSyntaxTree.ParseText($"public class Plugin{i:D4} : IPlugin;")))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(@"
[AutoCtor.ServiceProvider]
[AutoCtor.ScanSingleton(typeof(IPlugin), FromAssembliesOf = new[] { typeof(Container) })]
public partial class Container;
"));

        _driver = CSharpGeneratorDriver.Create(new AutoConstructSourceGenerator());

        _warmDriver = _driver.RunGenerators(_compilation);

        var edited = _compilation.SyntaxTrees.Single(t => t.ToString().Contains("class Plugin0001", StringComparison.Ordinal));
        _editedCompilation = _compilation.ReplaceSyntaxTree(edited,
            CSharpSyntaxTree.ParseText(edited.ToString()
            .Replace("{", "{ private bool _edited;", StringComparison.Ordinal)));
    }

    [Benchmark]
    public GeneratorDriver Scan() => _driver.RunGenerators(_compilation);

    [Benchmark]
    public GeneratorDriver IncrementalEdit() => _warmDriver.RunGenerators(_editedCompilation);
}

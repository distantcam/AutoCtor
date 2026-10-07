using AutoCtor;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace CompileBenchmarks;

[ShortRunJob]
public class Scanner
{
    [Params(500, 1000, 5000)]
    public int PluginCount { get; set; }

    private CSharpCompilation _compilation = null!;
    private GeneratorDriver _driver = null!;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        var frameworkReferences = await ReferenceAssemblies.Net.Net100
            .ResolveAsync(CSharpParseOptions.Default.Language, CancellationToken.None)
            .ConfigureAwait(false);

        _compilation = CSharpCompilation.Create(nameof(LocalCompare))
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
    }

    [Benchmark]
    public GeneratorDriver Scan() => _driver.RunGenerators(_compilation);
}

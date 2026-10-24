using System.Text;
using AutoCtor;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace CompileBenchmarks;

[Config(typeof(DefaultConfig))]
[MemoryDiagnoser]
public class Constructor
{
    [Params(100, 500)]
    public int ServiceCount { get; set; }

    [Params(100, 500)]
    public int TargetsCount { get; set; }

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

        _compilation = CSharpCompilation.Create(nameof(Constructor))
            .WithOptions(new(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(frameworkReferences)
            .AddReferences(MetadataReference
                .CreateFromFile(typeof(AutoConstructAttribute).Assembly.Location))
            .AddSyntaxTrees(Enumerable.Range(1, ServiceCount)
                .Select(i => CSharpSyntaxTree.ParseText($"public interface IService{i:D4};")))
            .AddSyntaxTrees(Enumerable.Range(1, TargetsCount).Select(TargetClassGenerator));

        _driver = CSharpGeneratorDriver.Create(new AutoConstructSourceGenerator());

        _warmDriver = _driver.RunGenerators(_compilation);
        var edited = _compilation.SyntaxTrees.Single(t => t.ToString().Contains("class Target0001", StringComparison.Ordinal));
        _editedCompilation = _compilation.ReplaceSyntaxTree(edited,
            CSharpSyntaxTree.ParseText(edited.ToString()
            .Replace("{", "{ private readonly IService0001 _edited;", StringComparison.Ordinal)));
    }

    private SyntaxTree TargetClassGenerator(int i)
    {
        var rng = new Random(i);

        var sb = new StringBuilder();

        sb.AppendLine("[AutoCtor.AutoConstruct]");
        sb.AppendLine($"public partial class Target{i:D4}");
        sb.AppendLine("{");

        var count = rng.Next(ServiceCount);
        var items = rng.GetItems(Enumerable.Range(1, ServiceCount).ToArray(), count);
        foreach (var item in items)
        {
            sb.Append($"    private readonly IService{item:D4} _service{item:D4};");
        }

        sb.AppendLine("}");

        return CSharpSyntaxTree.ParseText(sb.ToString());
    }

    [Benchmark]
    public GeneratorDriver Construct() => _driver.RunGenerators(_compilation);

    [Benchmark]
    public GeneratorDriver IncrementalEdit() => _warmDriver.RunGenerators(_editedCompilation);
}

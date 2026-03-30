using AutoCtor;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace AutoCtor.Benchmarks;

[MemoryDiagnoser]
public class AutoCtorBenchmarks
{
    private static IReadOnlyList<MetadataReference>? s_references;

    private CSharpCompilation _compilation = null!;
    private GeneratorDriver _warmDriver = null!;

    [Params(100, 500, 1000)]
    public int FileCount { get; set; }

    [Params(5, 10)]
    public int FieldCount { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        s_references ??= (await ReferenceAssemblies.NetStandard.NetStandard20
            .ResolveAsync(LanguageNames.CSharp, CancellationToken.None)
            .ConfigureAwait(false))
            .Add(MetadataReference.CreateFromFile(typeof(AutoConstructAttribute).Assembly.Location));

        _compilation = CSharpCompilation.Create(
            "BenchmarkAssembly",
            Enumerable.Range(0, FileCount)
                .Select(i => CSharpSyntaxTree.ParseText(BuildCode(i, FieldCount)))
                .ToArray(),
            s_references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        _warmDriver = CreateDriver().RunGenerators(_compilation);
    }

    [Benchmark(Baseline = true)]
    public GeneratorDriverRunResult Cold()
    {
        return CreateDriver().RunGenerators(_compilation).GetRunResult();
    }

    [Benchmark]
    public GeneratorDriverRunResult Cached()
    {
        _warmDriver = _warmDriver.RunGenerators(_compilation);
        return _warmDriver.GetRunResult();
    }

    private static CSharpGeneratorDriver CreateDriver()
    {
        return CSharpGeneratorDriver.Create(
            [new AutoConstructSourceGenerator().AsSourceGenerator()],
            parseOptions: CSharpParseOptions.Default);
    }

    private static string BuildCode(int fileIndex, int fieldCount)
    {
        var interfaces = string.Join(
            "\n",
            Enumerable.Range(1, fieldCount)
                .Select(i => $"public interface IService{fileIndex}_{i} {{ }}"));

        var fields = string.Join(
            "\n",
            Enumerable.Range(1, fieldCount)
                .Select(i => $"    private readonly IService{fileIndex}_{i} _service{i};"));

        return $$"""
            using AutoCtor;

            {{interfaces}}

            [AutoConstruct]
            public partial class TestService{{fileIndex}}
            {
            {{fields}}
            }
            """;
    }
}

using AutoCtor;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoCtor.Benchmarks;

[MemoryDiagnoser]
public class AutoCtorBenchmarks
{
    private const int ProjectCount = 20;
    private const int FilesPerProject = 50; // 20 × 50 = 1000 source files total

    private static readonly MetadataReference[] s_references;

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance", "CA1810:Initialize reference type static fields inline",
        Justification = "Requires Path.GetDirectoryName which cannot be used inline.")]
    static AutoCtorBenchmarks()
    {
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        s_references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(typeof(AutoConstructAttribute).Assembly.Location),
        ];
    }

    private CSharpCompilation[] _compilations = null!;
    private GeneratorDriver[] _warmDrivers = null!;

    [Params(1, 5, 10)]
    public int FieldCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _compilations = Enumerable.Range(0, ProjectCount)
            .Select(p => CSharpCompilation.Create(
                $"BenchmarkAssembly{p}",
                Enumerable.Range(0, FilesPerProject)
                    .Select(f => CSharpSyntaxTree.ParseText(BuildCode(p * FilesPerProject + f, FieldCount)))
                    .ToArray(),
                s_references,
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    nullableContextOptions: NullableContextOptions.Enable)))
            .ToArray();

        _warmDrivers = _compilations
            .Select(c => (GeneratorDriver)CreateDriver().RunGenerators(c))
            .ToArray();
    }

    [Benchmark(Baseline = true)]
    public void Cold()
    {
        foreach (var compilation in _compilations)
            CreateDriver().RunGenerators(compilation);
    }

    [Benchmark]
    public void Cached()
    {
        for (var i = 0; i < _compilations.Length; i++)
            _warmDrivers[i] = _warmDrivers[i].RunGenerators(_compilations[i]);
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

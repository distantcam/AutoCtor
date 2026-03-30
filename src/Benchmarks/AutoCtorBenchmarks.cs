using AutoCtor;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoCtor.Benchmarks;

[MemoryDiagnoser]
public class AutoCtorBenchmarks
{
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

    private CSharpCompilation _compilation = null!;
    private GeneratorDriver _warmDriver = null!;

    [Params(1, 100, 500, 1000)]
    public int FileCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _compilation = CSharpCompilation.Create(
            "BenchmarkAssembly",
            Enumerable.Range(0, FileCount)
                .Select(i => CSharpSyntaxTree.ParseText(BuildCode(i)))
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

    private static string BuildCode(int fileIndex)
    {
        return $$"""
            using AutoCtor;

            [AutoConstruct]
            public partial class TestService{{fileIndex}}
            {
                private readonly object _dependency;
            }
            """;
    }
}

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

internal class CompilationBuilder
{
    private const string DEFAULT_TARGET_FRAMEWORK = "net9.0";

    private ImmutableArray<ReferenceAssemblies> _nugetReferences;
    private ImmutableArray<MetadataReference> _references;
    private ImmutableArray<string> _codes;

    private CSharpParseOptions _parseOptions;
    private CSharpCompilationOptions _compilationOptions;

    public CSharpParseOptions ParseOptions => _parseOptions;

    public CompilationBuilder()
    {
        _nugetReferences = [];
        _references = [];
        _codes = [];

        _parseOptions = CSharpParseOptions.Default;
        _compilationOptions = new(OutputKind.DynamicallyLinkedLibrary);
    }

    private CompilationBuilder(CompilationBuilder other)
    {
        _nugetReferences = other._nugetReferences;
        _references = other._references;
        _codes = other._codes;

        _parseOptions = other._parseOptions;
        _compilationOptions = other._compilationOptions;
    }

    public async Task<CSharpCompilation> Build(string assemblyName, CancellationToken cancellationToken = default)
    {
        var nugetReferences = await _nugetReferences.ToAsyncEnumerable()
            .SelectMany<ReferenceAssemblies, MetadataReference>(static async (r, ct) =>
                await r.ResolveAsync(null, ct)
                .ConfigureAwait(false))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return CSharpCompilation.Create(assemblyName)
            .AddReferences(nugetReferences)
            .AddReferences(_references)
            .AddSyntaxTrees(_codes.Select(c => CSharpSyntaxTree.ParseText(c, _parseOptions)))
            .WithOptions(_compilationOptions);
    }

    public CompilationBuilder AddNugetReference(string id, string version, string targetFramework = DEFAULT_TARGET_FRAMEWORK, string path = "")
    {
        return new(this)
        {
            _nugetReferences = _nugetReferences.Add(new(
                targetFramework,
                new(id, version),
                Path.Join(string.IsNullOrEmpty(path) ? "lib" : path, targetFramework)
            ))
        };
    }

    public CompilationBuilder AddNetCoreReference(
        string targetFramework = DEFAULT_TARGET_FRAMEWORK,
        string? version = null)
        => AddNugetReference(
            "Microsoft.NETCore.App.Ref",
            version ?? TestFileHelper.GetPackageVersion("Microsoft.NETCore.App.Ref"),
            targetFramework,
            "ref");

    public CompilationBuilder AddAssemblyReference<T>()
    {
        return new(this)
        {
            _references = _references.Add(MetadataReference.CreateFromFile(typeof(T).Assembly.Location))
        };
    }

    public CompilationBuilder AddCompilationReference(Compilation compilation)
    {
        return new(this)
        {
            _references = _references.Add(compilation.ToMetadataReference())
        };
    }

    public CompilationBuilder AddCode(string code)
    {
        return new(this)
        {
            _codes = _codes.Add(code)
        };
    }

    public CompilationBuilder AddCodes(IEnumerable<string> codes)
    {
        return new(this)
        {
            _codes = _codes.AddRange(codes)
        };
    }

    public CompilationBuilder WithNullableContextOptions(NullableContextOptions options)
    {
        return new(this)
        {
            _compilationOptions = _compilationOptions.WithNullableContextOptions(options)
        };
    }

    public CompilationBuilder WithPreprocessorSymbols(IEnumerable<string>? preprocessorSymbols)
    {
        return new(this)
        {
            _parseOptions = _parseOptions.WithPreprocessorSymbols(preprocessorSymbols)
        };
    }

    public CompilationBuilder WithLanguageVersion(LanguageVersion langVersion)
    {
        return new(this)
        {
            _parseOptions = _parseOptions.WithLanguageVersion(langVersion)
        };
    }
}

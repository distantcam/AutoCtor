using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

internal class CompilationBuilder
{
    private ImmutableArray<ReferenceAssemblies> _nugetReferences;
    private ImmutableArray<MetadataReference> _references;
    private ImmutableArray<string> _codes;

    private CSharpParseOptions _parseOptions;
    private CSharpCompilationOptions _compilationOptions;

    private string _defaultTargetFramework = "net9.0";

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

    public async Task<CSharpCompilation> Build(string assemblyName)
    {
        var refTasks = _nugetReferences.Select(r => r.ResolveAsync(null, CancellationToken.None));
        await Task.WhenAll(refTasks);
        var nugetReferences = refTasks.SelectMany(t => t.Result);

        return CSharpCompilation.Create(assemblyName)
            .AddReferences(nugetReferences)
            .AddReferences(_references)
            .AddSyntaxTrees(_codes.Select(c => CSharpSyntaxTree.ParseText(c, _parseOptions)))
            .WithOptions(_compilationOptions);
    }

    public CompilationBuilder AddNugetReference(string id, string version, string targetFramework = "", string path = "")
    {
        var framework = string.IsNullOrEmpty(targetFramework) ? _defaultTargetFramework : targetFramework;

        return new(this)
        {
            _nugetReferences = _nugetReferences.Add(new(
                framework,
                new(id, version),
                Path.Join(string.IsNullOrEmpty(path) ? "lib" : path, framework)
            ))
        };
    }

    public CompilationBuilder AddNetCoreReference(string targetFramework = "", string version = "9.0.4")
        => AddNugetReference(
            "Microsoft.NETCore.App.Ref",
            version,
            targetFramework,
            "ref");

    public CompilationBuilder AddAssemblyReference<T>()
    {
        return new(this)
        {
            _references = _references.Add(MetadataReference.CreateFromFile(typeof(T).Assembly.Location))
        };
    }

    public CompilationBuilder AddCompilationReference(CSharpCompilation compilation)
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

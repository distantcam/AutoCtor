using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

public class CompilationBuilder
{
    private static readonly XDocument s_packageVersionDoc;
    static CompilationBuilder()
    {
        var srcDir = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;
        var packageVersionsFile = Path.Combine(srcDir?.FullName ?? "", "Directory.Packages.props");
        s_packageVersionDoc = XDocument.Load(packageVersionsFile);
    }

    private ImmutableArray<MetadataReference> _references;
    private ImmutableArray<string> _codes;

    private CSharpParseOptions _parseOptions;
    private CSharpCompilationOptions _compilationOptions;

    private string _targetFramework = "net10.0";

    public CSharpParseOptions ParseOptions => _parseOptions;

    public CompilationBuilder()
    {
        _references = [];
        _codes = [];

        _parseOptions = CSharpParseOptions.Default;
        _compilationOptions = new(OutputKind.DynamicallyLinkedLibrary);
    }

    private CompilationBuilder(CompilationBuilder other)
    {
        _references = other._references;
        _codes = other._codes;

        _parseOptions = other._parseOptions;
        _compilationOptions = other._compilationOptions;
    }

    public CSharpCompilation Build(string assemblyName)
    {
        return CSharpCompilation.Create(assemblyName)
            .AddReferences(_references)
            .AddSyntaxTrees(_codes.Select(c => CSharpSyntaxTree.ParseText(c, _parseOptions)))
            .WithOptions(_compilationOptions);
    }

    public CompilationBuilder AddReferences(params IEnumerable<MetadataReference> references)
    {
        return new(this)
        {
            _references = _references.AddRange(references)
        };
    }

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

    public CompilationBuilder AddCodes(params IEnumerable<string> codes)
    {
        return new(this)
        {
            _codes = _codes.AddRange(codes)
        };
    }

    public async Task<CompilationBuilder> AddNugetReference(string id, string path = "lib", CancellationToken cancellationToken = default)
    {
        var versionXml = s_packageVersionDoc.Root?.Descendants("PackageVersion")
            .FirstOrDefault(el => el.Attribute("Include")?.Value == id);
        var version = versionXml?.Attribute("Version")?.Value
            ?? throw new Exception($"{id} missing from Directory.Packages.props");
        var nugetReference = new ReferenceAssemblies(
            _targetFramework,
            new(id, version),
            Path.Combine(path, _targetFramework));
        var references = await nugetReference.ResolveAsync(_parseOptions.Language, cancellationToken)
            .ConfigureAwait(false);
        return new(this)
        {
            _references = _references.AddRange(references)
        };
    }

    public CompilationBuilder WithTargetFramework(string targetFramework)
    {
        return new(this)
        {
            _targetFramework = targetFramework
        };
    }

    public CompilationBuilder WithNullableContextOptions(NullableContextOptions options)
    {
        return new(this)
        {
            _compilationOptions = _compilationOptions.WithNullableContextOptions(options)
        };
    }

    public CompilationBuilder WithPreprocessorSymbols(params IEnumerable<string>? preprocessorSymbols)
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

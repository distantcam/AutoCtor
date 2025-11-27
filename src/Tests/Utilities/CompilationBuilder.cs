using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public class CompilationBuilder
{
    private ImmutableArray<MetadataReference> _references;
    private ImmutableArray<string> _codes;

    private CSharpParseOptions _parseOptions;
    private CSharpCompilationOptions _compilationOptions;

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

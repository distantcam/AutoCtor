﻿using System.Collections.Immutable;
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

    public CompilationBuilder AddNugetReference(string targetFramework, string id, string version, string path)
    {
        return new(this)
        {
            _nugetReferences = _nugetReferences.Add(new(
                targetFramework,
                new(id, version),
                path
            ))
        };
    }

    public CompilationBuilder AddNetCoreReference(string targetFramework = "net9.0", string version = "9.0.2")
        => AddNugetReference(
            targetFramework,
            "Microsoft.NETCore.App.Ref",
            version,
            Path.Join("ref", targetFramework));

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

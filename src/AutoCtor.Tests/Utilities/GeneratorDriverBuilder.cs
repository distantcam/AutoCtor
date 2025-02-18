using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

internal class GeneratorDriverBuilder
{
    private ImmutableArray<ISourceGenerator> _generators;
    private ImmutableDictionary<string, string> _analyzerOptions;

    public GeneratorDriverBuilder()
    {
        _generators = [];
        _analyzerOptions = ImmutableDictionary.Create<string, string>();
    }

    private GeneratorDriverBuilder(GeneratorDriverBuilder other)
    {
        _generators = other._generators;
        _analyzerOptions = other._analyzerOptions;
    }

    public GeneratorDriverBuilder AddGenerator(ISourceGenerator generator)
    {
        return new(this)
        {
            _generators = _generators.Add(generator)
        };
    }

    public GeneratorDriverBuilder AddGenerator(IIncrementalGenerator generator)
    {
        return new(this)
        {
            _generators = _generators.Add(generator.AsSourceGenerator())
        };
    }

    public GeneratorDriverBuilder WithAnalyzerOptions(IDictionary<string, string> analyzerOptions)
    {
        return new(this)
        {
            _analyzerOptions = analyzerOptions.ToImmutableDictionary()
        };
    }

    public GeneratorDriver Build(CSharpParseOptions parseOptions)
    {
#if ROSLYN_3
        return CSharpGeneratorDriver.Create(_generators,
            parseOptions: parseOptions,
            optionsProvider: new TestAnalyzerConfigOptionsProvider(_analyzerOptions)
        );
#elif ROSLYN_4_0
        return CSharpGeneratorDriver.Create(_generators,
            parseOptions: parseOptions,
            optionsProvider: new TestAnalyzerConfigOptionsProvider(_analyzerOptions),
            driverOptions: new GeneratorDriverOptions(
                disabledOutputs: IncrementalGeneratorOutputKind.None
            ));
#elif ROSLYN_4_4
        return CSharpGeneratorDriver.Create(_generators,
            parseOptions: parseOptions,
            optionsProvider: new TestAnalyzerConfigOptionsProvider(_analyzerOptions),
            driverOptions: new GeneratorDriverOptions(
                disabledOutputs: IncrementalGeneratorOutputKind.None,
                trackIncrementalGeneratorSteps: true
            ));
#endif
    }

    private class TestAnalyzerConfigOptionsProvider(
        ImmutableDictionary<string, string> analyzerConfigOptions
    ) : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = new TestAnalyzerConfigOptions(analyzerConfigOptions);
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;
    }

    private class TestAnalyzerConfigOptions(
        ImmutableDictionary<string, string> analyzerConfigOptions
    ) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
            => analyzerConfigOptions.TryGetValue(key, out value);
    }
}

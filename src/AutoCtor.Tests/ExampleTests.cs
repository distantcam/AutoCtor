using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit.Abstractions;

namespace AutoCtor.Tests;

[UsesVerify]
public class ExampleTests
{
    private readonly VerifySettings _codeVerifySettings;

    public ExampleTests()
    {
        _codeVerifySettings = new();
        _codeVerifySettings.ScrubLinesContaining("Version:", "SHA:", "GeneratedCodeAttribute");
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public Task ExamplesGeneratedCode(CodeFileTheoryData theoryData)
    {
        var baseDir = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;
        var exampleInterfaces = File.ReadAllText(Path.Combine(baseDir.FullName, "Examples", "IExampleInterfaces.cs"));

        var compilation = Compile(theoryData.Code, exampleInterfaces);
        var generator = new AutoConstructSourceGenerator();
        var driver = CreateDriver(compilation, generator).RunGenerators(compilation);

        return Verify(driver, _codeVerifySettings)
            .UseDirectory(Path.Combine("Examples", "Verified"))
            .UseTypeName(theoryData.Name);
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public void CodeCompilesProperly(CodeFileTheoryData theoryData)
    {
        var ignoredWarnings = new string[] {
            "CS0414" // Ignore unused fields
        };

        var baseDir = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;
        var exampleInterfaces = File.ReadAllText(Path.Combine(baseDir.FullName, "Examples", "IExampleInterfaces.cs"));

        var compilation = Compile(theoryData.Code, exampleInterfaces);
        var generator = new AutoConstructSourceGenerator();
        CreateDriver(compilation, generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        outputCompilation.GetDiagnostics()
            .Where(d => !ignoredWarnings.Contains(d.Id))
            .Should().BeEmpty();
    }

#if ROSLYN_3_11
    private static GeneratorDriver CreateDriver(Compilation c, params ISourceGenerator[] generators)
        => CSharpGeneratorDriver.Create(generators, parseOptions: c.SyntaxTrees.FirstOrDefault().Options as CSharpParseOptions);
#elif ROSLYN_4_0 || ROSLYN_4_4
    private static GeneratorDriver CreateDriver(Compilation c, params IIncrementalGenerator[] generators)
        => CSharpGeneratorDriver.Create(generators).WithUpdatedParseOptions(c.SyntaxTrees.FirstOrDefault().Options as CSharpParseOptions);
#endif

    private static CSharpCompilation Compile(params string[] code)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .Concat(new[] { MetadataReference.CreateFromFile(Path.Combine(Environment.CurrentDirectory, "AutoCtor.Attributes.dll")) });

#if ROSLYN_3_11
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
#elif ROSLYN_4_0 || ROSLYN_4_4
        var options = CSharpParseOptions.Default;
#endif

        return CSharpCompilation.Create(
            "AutoCtorTest",
            code.Select(c => CSharpSyntaxTree.ParseText(c, options)),
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary
            ));
    }

    public static IEnumerable<object[]> GetExamples()
    {
        var baseDir = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;

        if (baseDir == null)
        {
            yield break;
        }

        var examples = Directory.GetFiles(Path.Combine(baseDir.FullName, "Examples"), "*.cs");
        foreach (var example in examples)
        {
            if (example.Contains(".g.") || example.Contains("IExampleInterfaces"))
                continue;

            var code = File.ReadAllText(example);
            yield return new object[] {
                new CodeFileTheoryData {
                    Code = code,
                    Name = Path.GetFileNameWithoutExtension(example)
                }
            };
        }
    }

    public class CodeFileTheoryData : IXunitSerializable
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>("Name");
            Code = info.GetValue<string>("Code");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("Name", Name);
            info.AddValue("Code", Code);
        }

        public override string ToString() => Name + ".cs";
    }
}

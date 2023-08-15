using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit.Abstractions;

namespace AutoCtor.Tests;

[UsesVerify]
public class SourceGenerationTests
{
    private readonly VerifySettings _codeVerifySettings;

    public SourceGenerationTests()
    {
        _codeVerifySettings = new();
        _codeVerifySettings.ScrubLinesContaining("Version:", "SHA:", "GeneratedCodeAttribute");
    }

    [Theory]
    [InlineData("[AutoConstruct]")]
    [InlineData("[AutoConstructAttribute]")]
    public Task AttributeTest(string attribute)
    {
        var code = @$"using AutoCtor;
{attribute}public partial class AttributeTestClass {{}}
";
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver, _codeVerifySettings)
            .UseDirectory("Verified")
            .UseParameters(attribute);
    }

    [Theory]
    [InlineData("bool")]
    [InlineData("int")]
    [InlineData("string")]
    [InlineData("object")]
    [InlineData("IEnumerable<string>")]
    [InlineData("List<int>")]
    [InlineData("int[]")]
    [InlineData("int?")]
    [InlineData("Nullable<int>")]
    public Task TypesTest(string type)
    {
        var code = @$"
using System;
using System.Collections.Generic;
using AutoCtor;
[AutoConstruct]public partial class TypesTestClass {{ private readonly {type} _item; }}
";
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver, _codeVerifySettings)
            .UseDirectory("Verified")
            .UseParameters(type);
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public Task ExamplesGeneratedCode(CodeFileTheoryData theoryData)
    {
        var baseDir = new DirectoryInfo(Environment.CurrentDirectory)?.Parent?.Parent?.Parent;
        var exampleInterfaces = File.ReadAllText(Path.Combine(baseDir.FullName, "Examples", "IExampleInterfaces.cs"));

        var compilation = Compile(theoryData.Code, exampleInterfaces);
        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

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
        CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics()
            .Where(d => !ignoredWarnings.Contains(d.Id))
            .Should().BeEmpty();
    }

    private static CSharpCompilation Compile(params string[] code)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .Concat(new[] { MetadataReference.CreateFromFile(Path.Combine(Environment.CurrentDirectory, "AutoCtor.Attributes.dll")) });

        return CSharpCompilation.Create(
            "AutoCtorTest",
            code.Select(c => CSharpSyntaxTree.ParseText(c)),
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

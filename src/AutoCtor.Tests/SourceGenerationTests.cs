using AutoCtor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

[UsesVerify]
public class SourceGenerationTests
{
    [Theory]
    [InlineData("[AutoConstruct]")]
    [InlineData("[AutoConstructAttribute]")]
    public Task AttributeTest(string attribute)
    {
        var code = @$"
{attribute}public partial class AttributeTestClass {{}}
";
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver).UseParameters(attribute);
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

[AutoConstruct]public partial class TypesTestClass {{ private readonly {type} _item; }}
";
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver).UseParameters(type);
    }

    [Theory]
    [MemberData(nameof(GetExamples))]
    public Task ExamplesTest(string code, string name)
    {
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver)
            .UseDirectory("Examples")
            .UseTypeName(name);
    }

    private static CSharpCompilation Compile(params string[] code)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        return CSharpCompilation.Create(
            "AutoCtorTest",
            code.Select(c => CSharpSyntaxTree.ParseText(c)),
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
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
            var code = File.ReadAllText(example);
            yield return new object[] { code, Path.GetFileNameWithoutExtension(example) };
        }
    }
}

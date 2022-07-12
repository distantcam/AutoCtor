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
{attribute}public partial class TestClass {{}}
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

[AutoConstruct]public partial class TestClass {{ private readonly {type} _item; }}
";
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver).UseParameters(type);
    }

    [Fact]
    public Task FriendlyParameterNamesTest()
    {
        var code = @"
[AutoConstruct]public partial class TestClass
{
    private readonly int _underscorePrefix;
    private readonly int camelCase;
    private readonly int PascalCase;
}";
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver);
    }

    [Fact]
    public Task NamespaceTest()
    {
        var code = @"
namespace TestNamespace
{
    [AutoConstruct]public partial class TestClass
    {
        private readonly int _item;
    }
}";
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver);
    }

    [Fact]
    public Task NestedClassTest()
    {
        var code = @"
public partial class OuterClass
{
    [AutoConstruct]public partial class TestClass
    {
        private readonly int _item;
    }
}";
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver);
    }

    [Fact]
    public Task NamespaceDoubleNestedClassTest()
    {
        var code = @"
namespace TestNamespace
{
    public partial class OuterClass1
    {
        public partial class OuterClass2
        {
            [AutoConstruct]public partial class TestClass
            {
                private readonly int _item;
            }
        }
    }
}";
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver);
    }

    [Fact]
    public Task GenericClassTest()
    {
        var code = @"
[AutoConstruct]public partial class TestClass<T>
{
    private readonly T _item;
}";
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver);
    }

    [Fact]
    public Task RecordTest()
    {
        var code = @"
[AutoConstruct]public partial record TestRecord
{
    private readonly T _item;
}";
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver);
    }

    [Fact]
    public Task MixedNestedClassAndRecordTest()
    {
        var code = @"
public partial class OuterClass1
{
    public partial record OuterRecord1
    {
        public partial class OuterClass2
        {
            [AutoConstruct]public partial record TestRecord
            {
                private readonly int _item;
            }
        }
    }
}";
        var compilation = Compile(code);

        var generator = new AutoConstructSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);

        return Verify(driver);
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
}

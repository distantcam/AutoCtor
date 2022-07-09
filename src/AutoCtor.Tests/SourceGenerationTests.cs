using AutoCtor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

[UsesVerify]
public class SourceGenerationTests
{
    [Theory]
    [InlineData("[AutoConstruct]")]
    [InlineData("[AutoConstructAttribute]")]
    [InlineData("[AutoCtor.AutoConstruct]")]
    [InlineData("[AutoCtor.AutoConstructAttribute]")]
    public Task GeneratesWithAllAttributeOptions(string attribute)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@$"
{attribute}public class GeneratesWithAllAttributeOptions {{}}
");

        var compilation = CSharpCompilation.Create("AutoCtorTest", new[] { syntaxTree });
        var generator = new AutoConstructSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

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
    public Task GeneratesWithDifferentTypes(string type)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@$"
using System;
using System.Collections.Generic;

[AutoConstruct]public class GeneratesWithDifferentTypes {{ private readonly {type} _item; }}
");

        var compilation = CSharpCompilation.Create("AutoCtorTest", new[] { syntaxTree });
        var generator = new AutoConstructSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGenerators(compilation);

        return Verify(driver).UseParameters(type);
    }
}

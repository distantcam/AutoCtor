using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoCtor.Tests;

public class Issue73 : CompilationTestBase
{
    protected override CSharpCompilation Compile()
    {
        var projectACode = @"
namespace A
{
public interface Interface<T, U>{}
}
";
        var projectBCode = @"
using AutoCtor;
using A;
namespace B
{
public abstract class BaseClass<T, U, V>
{
    private readonly Interface<T, U> _interface;
    public BaseClass(Interface<T, U> i) => _interface = i;
}
[AutoConstruct]
public sealed partial class TheClass : BaseClass<object, int, string>{}
}
";

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .Concat(new[] { MetadataReference.CreateFromFile(Path.Combine(Environment.CurrentDirectory, "AutoCtor.Attributes.dll")) });

        var projectA = CSharpCompilation.Create(
            assemblyName: "ProjectA",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(projectACode) },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var projectB = CSharpCompilation.Create(
            assemblyName: "ProjectB",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(projectBCode) },
            references: references.Concat(new[] { projectA.ToMetadataReference() }),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return projectB;
    }
}

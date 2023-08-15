using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using AutoSource;

namespace AutoCtor;

[Generator(LanguageNames.CSharp)]
public class AutoConstructAttributeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static c =>
        {
            var source = new CodeBuilder();
            source.AppendHeader().AppendLine();
            source.AppendLine("#if AUTOCTOR_EMBED_ATTRIBUTES");
            source.AppendLine("namespace AutoCtor");
            source.StartBlock();
            source.AddCompilerGeneratedAttribute().AddGeneratedCodeAttribute();
            source.AppendLine("internal sealed class AutoConstructAttribute : System.Attribute");
            source.StartBlock();
            source.AppendLine("public AutoConstructAttribute()");
            source.StartBlock();
            source.EndBlock();
            source.AppendLine();
            source.AppendLine("public AutoConstructAttribute(string postConstructorMethod)");
            source.StartBlock();
            source.EndBlock();
            source.EndBlock();
            source.EndBlock();
            source.AppendLine("#endif");

            c.AddSource("AutoConstructAttribute.g.cs", source);
        });
    }
}

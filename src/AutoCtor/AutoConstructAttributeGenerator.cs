using Microsoft.CodeAnalysis;
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
            source.AppendLine("[global::System.AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]");
            source.AppendLine("internal sealed class AutoConstructAttribute : global::System.Attribute");
            source.StartBlock();
            source.AppendLine("public AutoConstructAttribute()");
            source.StartBlock().EndBlock();
            source.AppendLine();
            source.AppendLine("public AutoConstructAttribute(string postConstructorMethod)");
            source.StartBlock().EndBlock();
            source.EndBlock();

            source.AddCompilerGeneratedAttribute().AddGeneratedCodeAttribute();
            source.AppendLine("[global::System.AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]");
            source.AppendLine("internal sealed class AutoPostConstructAttribute : global::System.Attribute");
            source.StartBlock().EndBlock();

            source.EndBlock();
            source.AppendLine("#endif");

            c.AddSource("AutoConstructAttribute.g.cs", source);
        });
    }
}

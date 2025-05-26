using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AutoCtor;

[Generator(LanguageNames.CSharp)]
public partial class AttributeSourceGenerator
{
    private static class Emitter
    {
        public static string HintName = "AutoConstructAttribute.g.cs";

        public static SourceText GenerateSource()
        {
            var source = new CodeBuilder();
            source.AppendHeader().AppendLine();
            source.AppendLine("#if AUTOCTOR_EMBED_ATTRIBUTES");
            using (source.StartBlock("namespace AutoCtor"))
            {
                source.AddCompilerGeneratedAttribute().AddGeneratedCodeAttribute();
                using (source.StartBlock("internal enum GuardSetting"))
                {
                    source.AppendLine("Default,");
                    source.AppendLine("Disabled,");
                    source.AppendLine("Enabled");
                }

                source.AddCompilerGeneratedAttribute().AddGeneratedCodeAttribute();
                source.AppendLine("[global::System.AttributeUsage(global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]");
                source.AppendLine("internal sealed class AutoConstructAttribute : global::System.Attribute");
                using (source.StartBlock())
                {
                    source.AppendLine("public AutoConstructAttribute(GuardSetting guard = GuardSetting.Default)");
                    source.StartBlock().Dispose();
                }

                source.AddCompilerGeneratedAttribute().AddGeneratedCodeAttribute();
                source.AppendLine("[global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = false, Inherited = false)]");
                source.AppendLine("internal sealed class AutoPostConstructAttribute : global::System.Attribute");
                source.StartBlock().Dispose();

                source.AddCompilerGeneratedAttribute().AddGeneratedCodeAttribute();
                source.AppendLine("[global::System.AttributeUsage(global::System.AttributeTargets.Field | global::System.AttributeTargets.Property, AllowMultiple = false, Inherited = false)]");
                source.AppendLine("internal sealed class AutoConstructIgnoreAttribute : global::System.Attribute");
                source.StartBlock().Dispose();
            }
            source.AppendLine("#endif");

            return source;
        }
    }
}

using Microsoft.CodeAnalysis.Text;

namespace AutoCtor;

public partial class AutoConstructSourceGenerator
{
    private static class AttributeEmitter
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
                using (source.StartBlock("public enum GuardSetting"))
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
                    source.OpenBlock().CloseBlock();
                }

                source.AddCompilerGeneratedAttribute().AddGeneratedCodeAttribute();
                source.AppendLine("[global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = false, Inherited = false)]");
                source.AppendLine("internal sealed class AutoPostConstructAttribute : global::System.Attribute");
                source.OpenBlock().CloseBlock();
            }
            source.AppendLine("#endif");

            return source;
        }
    }
}

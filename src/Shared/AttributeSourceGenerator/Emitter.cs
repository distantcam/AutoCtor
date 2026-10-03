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
                source.AddGeneratedCodeAttribute();
                using (source.StartBlock("internal enum GuardSetting"))
                {
                    source.AppendLine("Default,");
                    source.AppendLine("Disabled,");
                    source.AppendLine("Enabled");
                }

                source.AddGeneratedCodeAttribute();
                EmitAttributeUsage(source, "Class", "Struct");
                using (StartAttribute(source, "AutoConstructAttribute"))
                {
                    source.AppendLine("public AutoConstructAttribute(GuardSetting guard = GuardSetting.Default) { }");
                }

                source.AddGeneratedCodeAttribute();
                EmitAttributeUsage(source, "Method");
                StartAttribute(source, "AutoPostConstructAttribute").Dispose();

                source.AddGeneratedCodeAttribute();
                EmitAttributeUsage(source, "Field", "Property");
                StartAttribute(source, "AutoConstructIgnoreAttribute").Dispose();

                source.AddGeneratedCodeAttribute();
                EmitAttributeUsage(source, "Field", "Property", "Parameter");
                using (StartAttribute(source, "AutoKeyedServiceAttribute"))
                {
                    source.AppendLine("public object Key { get; }");
                    source.AppendLine("public AutoKeyedServiceAttribute(object key) => Key = key;");
                }

                source.AddGeneratedCodeAttribute();
                EmitAttributeUsage(source, "Class");
                using (StartAttribute(source, "ServiceProviderAttribute"))
                {
                    source.AppendLine("public string Fallback { get; set; }");
                }

                source.AddGeneratedCodeAttribute();
                using (source.StartBlock("internal abstract class ServiceAttribute : global::System.Attribute"))
                {
                    source.AppendLine("public object Key { get; set; }");
                    source.AppendLine("public string Factory { get; set; }");
                }

                string[] lifetimes = ["Singleton", "Transient", "Scoped"];

                foreach (var lifetime in lifetimes)
                {
                    source.AddGeneratedCodeAttribute();
                    EmitAttributeUsage(source, true, false, "Class");
                    using (StartAttribute(source, lifetime + "Attribute", "ServiceAttribute"))
                    {
                        source.AppendLine($"public {lifetime}Attribute(global::System.Type service, global::System.Type implementation = null) {{ }}");
                    }
                }

                source.AddGeneratedCodeAttribute();
                EmitAttributeUsage(source, true, false, "Class");
                using (StartAttribute(source, "ImportAttribute"))
                {
                    source.AppendLine("public ImportAttribute(global::System.Type module) { }");
                }

                source.AddGeneratedCodeAttribute();
                source.AppendLine("[global::System.Flags]");
                using (source.StartBlock("internal enum ScanAs"))
                {
                    source.AppendLine("Service = 1,");
                    source.AppendLine("Self = 2,");
                    source.AppendLine("ImplementedInterfaces = 4");
                }

                using (source.StartBlock("internal abstract class ScanAttribute : global::System.Attribute"))
                {
                    source.AppendLine("public ScanAs As { get; set; }");
                    source.AppendLine("public global::System.Type[] FromAssembliesOf { get; set; }");
                    source.AppendLine("public string TypeNameFilter { get; set; }");
                }

                foreach (var lifetime in lifetimes)
                {
                    source.AddGeneratedCodeAttribute();
                    EmitAttributeUsage(source, true, false, "Class");
                    using (StartAttribute(source, $"Scan{lifetime}Attribute", "ScanAttribute"))
                    {
                        source.AppendLine($"public Scan{lifetime}Attribute(global::System.Type service = null) {{ }}");
                    }
                }

                source.AppendLine("#if AUTOCTOR_EMBED_GENERIC_ATTRIBUTES");
                foreach (var lifetime in lifetimes)
                {
                    source.AddGeneratedCodeAttribute();
                    EmitAttributeUsage(source, true, false, "Class");
                    source.AppendLine($"internal sealed class {lifetime}Attribute<TService> : ServiceAttribute {{ }}");
                    source.AddGeneratedCodeAttribute();
                    EmitAttributeUsage(source, true, false, "Class");
                    source.AppendLine($"internal sealed class {lifetime}Attribute<TService, TImplementation> : ServiceAttribute {{ }}");
                }
                source.AddGeneratedCodeAttribute();
                EmitAttributeUsage(source, true, false, "Class");
                source.AppendLine("internal sealed class ImportAttribute<TModule> : global::System.Attribute { }");
                source.AppendLine("#endif");
            }
            source.AppendLine("#endif");

            return source;
        }

        private static void EmitAttributeUsage(CodeBuilder source, params string[] targets)
            => EmitAttributeUsage(source, false, false, targets);

        private static void EmitAttributeUsage(CodeBuilder source, bool allowMultiple, bool inherited, params string[] targets)
        {
            var targetString = string.Join(" | ", targets.Select(t => $"global::System.AttributeTargets.{t}"));

            source.AppendLine($"[global::System.AttributeUsage({targetString}, AllowMultiple = {allowMultiple}, Inherited = {inherited})]");
        }

        private static IDisposable StartAttribute(CodeBuilder source, string typeName, string baseType = "global::System.Attribute")
        {
            source.AppendLine($"internal sealed class {typeName} : {baseType}");
            return source.StartBlock();
        }
    }
}

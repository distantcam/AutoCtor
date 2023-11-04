using Microsoft.CodeAnalysis;

namespace AutoCtor;

public partial class AutoConstructSourceGenerator
{
    private static class Diagnostics
    {
        public static readonly DiagnosticDescriptor AmbiguousMarkedPostConstructMethodWarning = new(
            id: "ACTR001",
            title: "Ambiguous marked post constructor method",
            messageFormat: "Only one method in a type should be marked with an [AutoPostConstruct] attribute",
            category: "AutoCtor",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PostConstructMethodNotVoidWarning = new(
            id: "ACTR002",
            title: "Post construct method must return void",
            messageFormat: "The method '{0}' must return void to be used as the post construct method",
            category: "AutoCtor",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PostConstructMethodHasOptionalArgsWarning = new(
            id: "ACTR003",
            title: "Post construct method must not have any optional arguments",
            messageFormat: "The method '{0}' must not have optional arguments to be used as the post construct method",
            category: "AutoCtor",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PostConstructMethodCannotBeGenericWarning = new(
            id: "ACTR004",
            title: "Post construct method must not be generic",
            messageFormat: "The method '{0}' must not be generic to be used as the post construct method",
            category: "AutoCtor",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}

using Microsoft.CodeAnalysis;

#if ROSLYN_3
using EmitterContext = Microsoft.CodeAnalysis.GeneratorExecutionContext;
#elif ROSLYN_4
using EmitterContext = Microsoft.CodeAnalysis.SourceProductionContext;
#endif

namespace AutoCtor;

internal static class Diagnostics
{
    public static void ReportDiagnostic(EmitterContext context, IHaveDiagnostics item, DiagnosticDescriptor diagnostic)
    {
        foreach (var loc in item.Locations)
            context.ReportDiagnostic(Diagnostic.Create(diagnostic, loc, item.ErrorName));
    }

    /// <summary>
    /// Id: ACTR001<br />
    /// Title: Ambiguous marked post constructor method
    /// </summary>
    public static readonly DiagnosticDescriptor AmbiguousMarkedPostConstructMethod = new DiagnosticDescriptor(
        id: "ACTR001",
        title: "Ambiguous marked post constructor method",
        messageFormat: "Only one method in a type should be marked with an [AutoPostConstruct] attribute",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR002<br />
    /// Title: Post construct method must return void
    /// </summary>
    public static readonly DiagnosticDescriptor PostConstructMethodNotVoid = new DiagnosticDescriptor(
        id: "ACTR002",
        title: "Post construct method must return void",
        messageFormat: "The method '{0}' must return void to be used as the post construct method",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR003<br />
    /// Title: Post construct method must not have any optional arguments
    /// </summary>
    public static readonly DiagnosticDescriptor PostConstructMethodHasOptionalArgs = new DiagnosticDescriptor(
        id: "ACTR003",
        title: "Post construct method must not have any optional arguments",
        messageFormat: "The parameter '{0}' must not be optional",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR004<br />
    /// Title: Post construct method must not be generic
    /// </summary>
    public static readonly DiagnosticDescriptor PostConstructMethodCannotBeGeneric = new DiagnosticDescriptor(
        id: "ACTR004",
        title: "Post construct method must not be generic",
        messageFormat: "The method '{0}' must not be generic to be used as the post construct method",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR005<br />
    /// Title: Post construct out or ref parameter must not be a keyed service
    /// </summary>
    public static readonly DiagnosticDescriptor PostConstructOutParameterCannotBeKeyed = new DiagnosticDescriptor(
        id: "ACTR005",
        title: "Post construct out or ref parameter must not be a keyed service",
        messageFormat: "The parameter '{0}' must not be a keyed service, or cannot be out or ref",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR006<br />
    /// Title: Post construct out or ref parameter must not match a keyed field
    /// </summary>
    public static readonly DiagnosticDescriptor PostConstructOutParameterMustNotMatchKeyedField = new DiagnosticDescriptor(
        id: "ACTR006",
        title: "Post construct out or ref parameter must not match a keyed field",
        messageFormat: "The field '{0}' must not be a keyed service when used as a post construct out or ref parameter",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoCtor;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AddAutoConstructAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Diagnostics.ACTR008_AddAutoConstruct);

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationContext =>
        {
            compilationContext.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.ClassDeclaration);
            compilationContext.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.StructDeclaration);
            compilationContext.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.RecordDeclaration);
            compilationContext.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.RecordStructDeclaration);
        });
    }

    private static void AnalyzeType(SyntaxNodeAnalysisContext context)
    {
        var typeSyntax = (TypeDeclarationSyntax)context.Node;

        if (Utilities.HasAutoConstructAttribute(typeSyntax))
            return;

        if (context.SemanticModel.GetDeclaredSymbol(typeSyntax, context.CancellationToken) is not INamedTypeSymbol type)
            return;

        // no constructors
        if (type.InstanceConstructors.Any(c => !c.IsImplicitlyDeclared))
            return;

        if (!Utilities.HasEligibleMember(type))
            return;

        foreach (var location in type.Locations)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.ACTR008_AddAutoConstruct,
                location,
                type.Name));
        }
    }
}

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoCtor;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseAutoConstructAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Diagnostics.ACTR007_UseAutoConstruct);

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationContext =>
        {
            var autoConstructType = compilationContext.Compilation
                .GetTypeByMetadataName(AttributeNames.AutoConstruct);
            if (autoConstructType is null)
                return;

            compilationContext.RegisterSyntaxNodeAction(
                ctx => AnalyzeConstructor(ctx, autoConstructType),
                SyntaxKind.ConstructorDeclaration);
        });
    }

    private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol autoConstructType)
    {
        var ctorSyntax = (ConstructorDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(ctorSyntax, context.CancellationToken) is not IMethodSymbol ctor)
            return;

        var type = ctor.ContainingType;

        // Skip implicitly declared constructors
        if (ctor.IsImplicitlyDeclared)
            return;

        if (IsEligibleConstructor(ctor, ctorSyntax, type, context.SemanticModel, out var location))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Diagnostics.ACTR007_UseAutoConstruct,
                location,
                type.Name));
        }
    }

    private static bool IsEligibleConstructor(
        IMethodSymbol ctor,
        ConstructorDeclarationSyntax ctorSyntax,
        INamedTypeSymbol type,
        SemanticModel semanticModel,
        out Location location)
    {
        location = Location.None;

        // Not public
        if (ctor.DeclaredAccessibility != Accessibility.Public)
            return false;

        // No attributes on constructor
        if (ctor.GetAttributes().Any())
            return false;

        location = ctorSyntax.Identifier.GetLocation();

        // No initializer (: base() or : this())
        if (ctorSyntax.Initializer is not null)
            return false;

        // No empty constructors
        if (ctorSyntax.Body is null || ctorSyntax.Body.Statements.Count == 0)
            return false;

        // No parameter with explicit default
        foreach (var param in ctor.Parameters)
        {
            if (param.HasExplicitDefaultValue)
                return false;
        }

        var assignedMembers = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);

        // Every eligible member in the type must be covered
        var eligibleMembers = GetEligibleMembers(type);

        // Every statement must be a qualifying assignment
        foreach (var statement in ctorSyntax.Body.Statements)
        {
            if (statement is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment })
                return false;

            ISymbol? member = null;
            if (assignment.Left is MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } memberAccess)
                member = semanticModel.GetSymbolInfo(memberAccess.Name).Symbol;
            else if (assignment.Left is IdentifierNameSyntax ident)
                member = semanticModel.GetSymbolInfo(ident).Symbol;

            if (member is null || !eligibleMembers.Contains(member, SymbolEqualityComparer.Default))
                return false;

            if (assignment.Right is not IdentifierNameSyntax rightIdent)
                return false;
            if (semanticModel.GetSymbolInfo(rightIdent).Symbol is not IParameterSymbol param)
                return false;
            if (!SymbolEqualityComparer.Default.Equals(param.ContainingSymbol, ctor))
                return false;

            assignedMembers[member] = param.Name;
        }

        if (eligibleMembers.Count != assignedMembers.Count)
            return false;
        foreach (var m in eligibleMembers)
        {
            if (!assignedMembers.ContainsKey(m))
                return false;
        }

        // Every constructor parameter must be used in exactly one assignment
        if (ctor.Parameters.Length != assignedMembers.Count)
            return false;
        foreach (var param in ctor.Parameters)
        {
            if (!assignedMembers.ContainsValue(param.Name))
                return false;
        }

        return true;
    }

    private static List<ISymbol> GetEligibleMembers(INamedTypeSymbol type)
    {
        var result = new List<ISymbol>();
        foreach (var member in type.GetMembers())
        {
            if (member is IFieldSymbol field && ModelUtilities.IsValidField(field))
                result.Add(member);

            if (member is IPropertySymbol property && ModelUtilities.IsValidProperty(property))
                result.Add(member);
        }
        return result;
    }
}

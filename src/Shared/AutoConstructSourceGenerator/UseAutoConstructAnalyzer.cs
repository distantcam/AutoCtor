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

        // Skip if the type already has [AutoConstruct]
        if (type.GetAttributes()
            .Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, autoConstructType)))
            return;

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

        // No attributes on constructor
        if (ctor.GetAttributes().Any())
            return false;

        location = ctorSyntax.Identifier.GetLocation();

        // No initializer (: base() or : this())
        if (ctorSyntax.Initializer is not null)
            return false;

        if (ctorSyntax.Body is null)
            return false;

        if (ctorSyntax.Body.Statements.Count == 0)
            return false;

        var assignedMembers = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);

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

            if (member is null || !IsEligibleMember(member, type))
                return false;

            if (assignment.Right is not IdentifierNameSyntax rightIdent)
                return false;
            if (semanticModel.GetSymbolInfo(rightIdent).Symbol is not IParameterSymbol param)
                return false;
            if (!SymbolEqualityComparer.Default.Equals(param.ContainingSymbol, ctor))
                return false;

            assignedMembers[member] = param.Name;
        }

        // Every eligible member in the type must be covered
        var eligibleMembers = GetEligibleMembers(type);
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

    internal static bool IsEligibleMember(ISymbol member, INamedTypeSymbol containingType)
    {
        if (member is IFieldSymbol field)
        {
            if (!field.IsReadOnly || field.IsStatic || !field.CanBeReferencedByName || field.IsImplicitlyDeclared)
                return false;
            if (field.AssociatedSymbol is not null)
                return false; // compiler-generated backing field
            if (HasIgnoreAttribute(field.GetAttributes()))
                return false;

            var syntaxRef = field.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxRef?.GetSyntax() is VariableDeclaratorSyntax { Initializer: not null })
                return false;

            return true;
        }

        if (member is IPropertySymbol property)
        {
            if (property.IsStatic || !property.CanBeReferencedByName || property.IsImplicitlyDeclared)
                return false;
            if (HasIgnoreAttribute(property.GetAttributes()))
                return false;

            // Must have a compiler-generated backing field (i.e. be an auto-property)
            var hasBackingField = containingType.GetMembers()
                .OfType<IFieldSymbol>()
                .Any(f => SymbolEqualityComparer.Default.Equals(f.AssociatedSymbol, property));
            if (!hasBackingField)
                return false;

            var propertySyntax = property.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault();
            if (propertySyntax is null)
                return false;

            // Expression body (e.g. "public string X => ...") is not eligible
            if (property.IsReadOnly && propertySyntax.AccessorList is null)
                return false;

            // Getter with body only (e.g. "get { return _x; }") is not eligible
            if (property.IsReadOnly
                && propertySyntax.AccessorList?.Accessors.Count == 1
                && propertySyntax.AccessorList.Accessors[0].IsKind(SyntaxKind.GetAccessorDeclaration)
                && propertySyntax.AccessorList.Accessors[0].Body is not null)
                return false;

            if (propertySyntax.Initializer is not null)
                return false;

            // Must be get-only, init, or required
            var isInit = propertySyntax.AccessorList?.Accessors
                .Any(a => a.IsKind(SyntaxKind.InitAccessorDeclaration)) == true;
            if (!property.IsReadOnly &&
#if ROSLYN_4_4
                !property.IsRequired &&
#endif
                !isInit)
                return false;

            return true;
        }

        return false;
    }

    private static bool HasIgnoreAttribute(IEnumerable<AttributeData> attributes) =>
        attributes.Any(a => a.AttributeClass?.ToDisplayString() == AttributeNames.AutoConstructIgnore);

    private static List<ISymbol> GetEligibleMembers(INamedTypeSymbol type)
    {
        var result = new List<ISymbol>();
        foreach (var member in type.GetMembers())
        {
            if (IsEligibleMember(member, type))
                result.Add(member);
        }
        return result;
    }
}

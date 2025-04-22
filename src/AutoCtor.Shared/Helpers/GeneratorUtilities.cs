using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#if ROSLYN_3
using EmitterContext = Microsoft.CodeAnalysis.GeneratorExecutionContext;
#elif ROSLYN_4
using EmitterContext = Microsoft.CodeAnalysis.SourceProductionContext;
#endif

internal static class GeneratorUtilities
{
    public static readonly SymbolDisplayFormat HintSymbolDisplayFormat = new(
        globalNamespaceStyle:
            SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle:
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions:
            SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public static string ToParameterPrefix(this RefKind kind)
    {
        return kind switch
        {
            RefKind.Ref => "ref",
            RefKind.Out => "out",
            RefKind.In => "in",
            (RefKind)4 => "in", // ref readonly parameter introduced in C#12
            _ => string.Empty
        };
    }

    public static string EscapeKeywordIdentifier(this string identifier) =>
        SyntaxFacts.IsKeywordKind(SyntaxFacts.GetKeywordKind(identifier)) ? "@" + identifier : identifier;

    public static string? GetNamespace(ITypeSymbol type) =>
        type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace.ToString();

    public static EquatableList<string> GetTypeDeclarations(ITypeSymbol type)
    {
        var typeDeclarations = new Stack<string>();
        var currentType = type;
        while (currentType is not null)
        {
            var typeKeyword = currentType switch
            {
                { IsRecord: true, IsValueType: true } => "record struct",
                { IsRecord: true, IsValueType: false } => "record",
                { IsRecord: false, IsValueType: true } => "struct",
                { IsRecord: false, IsValueType: false } => "class",
                _ => string.Empty
            };

            var staticKeyword = currentType.IsStatic ? "static " : "";
            var typeName = currentType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            typeDeclarations.Push($"{staticKeyword}partial {typeKeyword} {typeName}");
            currentType = currentType.ContainingType;
        }
        return new EquatableList<string>(typeDeclarations);
    }

    public static string GetHintName(ITypeSymbol type)
    {
        return type.ToDisplayString(HintSymbolDisplayFormat)
            .Replace('<', '[')
            .Replace('>', ']');
    }

    public static bool IsTypeDeclaration(SyntaxNode node, CancellationToken cancellationToken)
        => node is TypeDeclarationSyntax { AttributeLists.Count: > 0 };

    public static bool IsMethodDeclaration(SyntaxNode node, CancellationToken cancellationToken)
        => node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };

    public static string? GetServiceKey(ISymbol symbol)
    {
        var keyedService = symbol.GetAttributes()
            .Where(a => a.AttributeClass?.ToDisplayString() == AttributeNames.AutoKeyedService
                || a.AttributeClass?.ToDisplayString() == "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute")
            .FirstOrDefault();

        if (keyedService != null)
            return keyedService.ConstructorArguments[0].ToCSharpString();

        return null;
    }
}

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
            _ => string.Empty
        };
    }

    public static string AsCommaSeparated<T>(this IEnumerable<T> items) =>
        string.Join(", ", items);

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
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class GeneratorUtilities
{
    public static readonly SymbolDisplayFormat HintSymbolDisplayFormat = new SymbolDisplayFormat(
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
}

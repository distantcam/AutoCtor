using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal readonly record struct MemberModel(
    EquatableTypeSymbol Type,
    string FriendlyName,
    string IdentifierName,
    string? KeyedService,

    bool IsReferenceType,
    bool IsNullableAnnotated
)
{
    public static MemberModel Create(IFieldSymbol field)
    {
        var friendlyName = field.Name.Length > 1 && field.Name[0] == '_'
            ? field.Name.Substring(1).EscapeKeywordIdentifier()
            : field.Name.EscapeKeywordIdentifier();

        return new MemberModel(
            Type: new(field.Type),
            FriendlyName: friendlyName,
            IdentifierName: "this." + field.Name.EscapeKeywordIdentifier(),
            KeyedService: GetServiceKey(field),

            IsReferenceType: field.Type.IsReferenceType,
            IsNullableAnnotated: field.Type.NullableAnnotation == NullableAnnotation.Annotated
        );
    }

    public static MemberModel Create(IPropertySymbol property)
    {
        var friendlyName = new string([char.ToLower(property.Name[0]), .. property.Name.Substring(1)]).EscapeKeywordIdentifier();

        return new MemberModel(
            Type: new(property.Type),
            FriendlyName: friendlyName,
            IdentifierName: "this." + property.Name.EscapeKeywordIdentifier(),
            KeyedService: GetServiceKey(property),

            IsReferenceType: property.Type.IsReferenceType,
            IsNullableAnnotated: property.Type.NullableAnnotation == NullableAnnotation.Annotated
        );
    }

    private static string? GetServiceKey(ISymbol symbol)
    {
        var keyedService = symbol.GetAttributes()
            .Where(a => a.AttributeClass?.ToDisplayString() == AttributeNames.FromKeyedServices)
            .FirstOrDefault();

        if (keyedService != null)
            return keyedService.ConstructorArguments[0].ToCSharpString();

        return null;
    }
}

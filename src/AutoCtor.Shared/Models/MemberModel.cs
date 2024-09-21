using Microsoft.CodeAnalysis;

internal record struct MemberModel(
    EquatableTypeSymbol Type,
    string FriendlyName,
    string IdentifierName,

    bool IsReferenceType
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
            IdentifierName: field.Name.EscapeKeywordIdentifier(),

            IsReferenceType: field.Type.IsReferenceType
        );
    }

    public static MemberModel Create(IPropertySymbol property)
    {
        var friendlyName = new string([char.ToLower(property.Name[0]), .. property.Name.Substring(1)]).EscapeKeywordIdentifier();

        return new MemberModel(
            Type: new(property.Type),
            FriendlyName: friendlyName,
            IdentifierName: property.Name.EscapeKeywordIdentifier(),

            IsReferenceType: property.Type.IsReferenceType
        );
    }
}

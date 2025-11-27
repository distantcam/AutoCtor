using Microsoft.CodeAnalysis;
using static ModelUtilities;

internal readonly record struct MemberModel(
    EquatableTypeSymbol Type,
    string FriendlyName,
    string IdentifierName,
    string ErrorName,
    string? KeyedService,

    bool IsReferenceType,
    bool IsNullableAnnotated,

    EquatableList<Location> Locations
) : IHaveDiagnostics
{
    public static MemberModel Create(IFieldSymbol field)
    {
        var checkLength = field.Name.Length - 1;
        var i = 0;
        while (checkLength > i && !char.IsLetter(field.Name[i]))
            i++;
        if (i > 0 && !char.IsLetter(field.Name[i]))
            i--;
        var friendlyName = char.IsUpper(field.Name[i])
            ? char.ToLower(field.Name[i]) + field.Name.Substring(i + 1)
            : field.Name.Substring(i);
        friendlyName = friendlyName.EscapeKeywordIdentifier();

        return new MemberModel(
            Type: new(field.Type),
            FriendlyName: friendlyName,
            IdentifierName: "this." + field.Name.EscapeKeywordIdentifier(),
            ErrorName: field.Name,
            KeyedService: GetServiceKey(field),

            IsReferenceType: field.Type.IsReferenceType,
            IsNullableAnnotated: field.Type.NullableAnnotation == NullableAnnotation.Annotated,

            Locations: new(field.Locations)
        );
    }

    public static MemberModel Create(IPropertySymbol property)
    {
        var friendlyName = new string([char.ToLower(property.Name[0]), .. property.Name.Substring(1)]).EscapeKeywordIdentifier();

        return new MemberModel(
            Type: new(property.Type),
            FriendlyName: friendlyName,
            IdentifierName: "this." + property.Name.EscapeKeywordIdentifier(),
            ErrorName: property.Name,
            KeyedService: GetServiceKey(property),

            IsReferenceType: property.Type.IsReferenceType,
            IsNullableAnnotated: property.Type.NullableAnnotation == NullableAnnotation.Annotated,

            Locations: new(property.Locations)
        );
    }
}

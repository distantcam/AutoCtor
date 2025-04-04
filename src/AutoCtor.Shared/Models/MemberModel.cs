﻿using Microsoft.CodeAnalysis;

internal readonly record struct MemberModel(
    EquatableTypeSymbol Type,
    string FriendlyName,
    string IdentifierName,

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

            IsReferenceType: property.Type.IsReferenceType,
            IsNullableAnnotated: property.Type.NullableAnnotation == NullableAnnotation.Annotated
        );
    }
}

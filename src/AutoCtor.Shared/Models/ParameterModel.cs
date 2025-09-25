using Microsoft.CodeAnalysis;

internal readonly record struct ParameterModel(
    RefKind RefKind,
    string Name,
    string ErrorName,
    string? KeyedService,
    bool HasExplicitDefaultValue,
    string ExplicitDefaultValue,
    bool IsOutOrRef,
    EquatableList<Location> Locations,
    EquatableTypeSymbol Type
) : IHaveDiagnostics
{
    public static ParameterModel Create(IParameterSymbol parameter)
    {
        var defaultValue = "";
        if (parameter.HasExplicitDefaultValue)
        {
            if (parameter.ExplicitDefaultValue is string s)
                defaultValue = $"\"{s}\"";

            else if (parameter.ExplicitDefaultValue is not null
                && parameter.Type is INamedTypeSymbol ptype
                && ptype.TypeKind == TypeKind.Enum)
                defaultValue = $"({ptype.ToDisplayString(FullyQualifiedFormat)}){parameter.ExplicitDefaultValue}";

            else if (parameter.ExplicitDefaultValue is not null)
                defaultValue = parameter.ExplicitDefaultValue.ToString();

            else
                defaultValue = "null";
        }

        return new(
            RefKind: parameter.RefKind,
            Name: parameter.Name.EscapeKeywordIdentifier(),
            ErrorName: parameter.Name,
            KeyedService: ModelUtilities.GetServiceKey(parameter),
            HasExplicitDefaultValue: parameter.HasExplicitDefaultValue,
            ExplicitDefaultValue: defaultValue,
            IsOutOrRef: parameter.RefKind == RefKind.Ref
                || parameter.RefKind == RefKind.Out,
            Locations: new(parameter.Locations),
            Type: new(parameter.Type)
        );
    }

    public bool Equals(ParameterModel other)
    {
        return EqualityComparer<string>.Default.Equals(Name, other.Name)
            && EqualityComparer<string?>.Default.Equals(KeyedService, other.KeyedService)
            && EqualityComparer<EquatableTypeSymbol>.Default.Equals(Type, other.Type)
            && EqualityComparer<bool>.Default.Equals(HasExplicitDefaultValue, other.HasExplicitDefaultValue);
    }

    public override int GetHashCode()
    {
        return ((EqualityComparer<string>.Default.GetHashCode(Name) * -1521134295
            + EqualityComparer<string?>.Default.GetHashCode(KeyedService)) * -1521134295
            + EqualityComparer<EquatableTypeSymbol>.Default.GetHashCode(Type)) * -1521134295
            + EqualityComparer<bool>.Default.GetHashCode(HasExplicitDefaultValue);
    }
}

using Microsoft.CodeAnalysis;

internal readonly record struct ParameterModel(
    RefKind RefKind,
    string Name,
    string ErrorName,
    string? KeyedService,
    bool IsOptional,
    bool IsOutOrRef,
    EquatableList<Location> Locations,
    EquatableTypeSymbol Type
) : IHaveDiagnostics
{
    public static ParameterModel Create(IParameterSymbol parameter)
    {
        return new(
            RefKind: parameter.RefKind,
            Name: parameter.Name.EscapeKeywordIdentifier(),
            ErrorName: parameter.Name,
            KeyedService: GeneratorUtilities.GetServiceKey(parameter),
            IsOptional: parameter.IsOptional,
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
            && EqualityComparer<EquatableTypeSymbol>.Default.Equals(Type, other.Type);
    }

    public override int GetHashCode()
    {
        return (EqualityComparer<string>.Default.GetHashCode(Name) * -1521134295
            + EqualityComparer<string?>.Default.GetHashCode(KeyedService)) * -1521134295
            + EqualityComparer<EquatableTypeSymbol>.Default.GetHashCode(Type);
    }
}

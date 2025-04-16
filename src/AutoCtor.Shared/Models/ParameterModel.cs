using Microsoft.CodeAnalysis;

internal readonly record struct ParameterModel(
    RefKind RefKind,
    string Name,
    string? KeyedService,
    EquatableTypeSymbol Type)
{
    public static ParameterModel Create(IParameterSymbol parameter) =>
        new(
            parameter.RefKind,
            parameter.Name.EscapeKeywordIdentifier(),
            null,
            new(parameter.Type)
        );

    public bool Equals(ParameterModel other)
    {
        return EqualityComparer<string>.Default.Equals(Name, other.Name)
            && EqualityComparer<EquatableTypeSymbol>.Default.Equals(Type, other.Type);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<string>.Default.GetHashCode(Name) * -1521134295
            + EqualityComparer<EquatableTypeSymbol>.Default.GetHashCode(Type);
    }
}

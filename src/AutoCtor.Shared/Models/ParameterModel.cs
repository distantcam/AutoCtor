using Microsoft.CodeAnalysis;

internal record struct ParameterModel(string Name, EquatableTypeSymbol Type)
{
    public static ParameterModel Create(IParameterSymbol parameter) =>
        new(parameter.Name.EscapeKeywordIdentifier(), new(parameter.Type));
}

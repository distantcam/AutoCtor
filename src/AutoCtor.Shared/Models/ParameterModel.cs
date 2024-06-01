using Microsoft.CodeAnalysis;

internal record struct ParameterModel(string Name, string TypeName)
{
    public static ParameterModel Create(IParameterSymbol parameter) =>
        new(parameter.Name.EscapeKeywordIdentifier(), parameter.Type.ToDisplayString(FullyQualifiedFormat));
}

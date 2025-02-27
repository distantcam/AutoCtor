using Microsoft.CodeAnalysis;

internal readonly record struct PostCtorModel(
    string TypeKey,
    string Name,
    string ErrorName,
    bool ReturnsVoid,
    bool HasOptionalParameters,
    bool IsGenericMethod,

    EquatableList<ParameterModel> Parameters,
    EquatableList<Location> Locations
)
{
    public static PostCtorModel Create(IMethodSymbol method)
    {
        return new(
            TypeKey: TypeModel.CreateKey(method.ContainingType),
            Name: method.Name,
            ErrorName: method.ToDisplayString(CSharpShortErrorMessageFormat),
            ReturnsVoid: method.ReturnsVoid,
            HasOptionalParameters: method.Parameters.Any(static p => p.IsOptional),
            IsGenericMethod: method.IsGenericMethod,

            Parameters: new(method.Parameters.Select(ParameterModel.Create)),
            Locations: new(method.Locations)
        );
    }
}

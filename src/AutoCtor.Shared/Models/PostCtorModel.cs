using Microsoft.CodeAnalysis;

internal readonly record struct PostCtorModel(
    string TypeKey,
    string Name,
    string ErrorName,
    bool ReturnsVoid,
    bool IsGenericMethod,

    EquatableList<ParameterModel> Parameters,
    EquatableList<Location> Locations
) : IHaveDiagnostics
{
    public static PostCtorModel Create(IMethodSymbol method)
    {
        return new(
            TypeKey: TypeModel.CreateKey(method.ContainingType),
            Name: method.Name,
            ErrorName: method.Name,
            ReturnsVoid: method.ReturnsVoid,
            IsGenericMethod: method.IsGenericMethod,

            Parameters: new(method.Parameters.Select(ParameterModel.Create)),
            Locations: new(method.Locations)
        );
    }
}

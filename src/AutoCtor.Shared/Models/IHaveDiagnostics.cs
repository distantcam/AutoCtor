using Microsoft.CodeAnalysis;

internal interface IHaveDiagnostics
{
    string ErrorName { get; }
    EquatableList<Location> Locations { get; }
}

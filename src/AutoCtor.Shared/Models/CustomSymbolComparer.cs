using Microsoft.CodeAnalysis;

internal class CustomSymbolComparer : IEqualityComparer<ISymbol>
{
    public static readonly CustomSymbolComparer Default = new();

    public int GetHashCode(ISymbol obj) =>
        obj is null ? 0 : obj.ToDisplayString().GetHashCode();
    public bool Equals(ISymbol x, ISymbol y) =>
        x is null ? y is null : x.ToDisplayString().Equals(y.ToDisplayString());
}

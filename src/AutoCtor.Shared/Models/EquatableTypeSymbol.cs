using Microsoft.CodeAnalysis;

internal class EquatableTypeSymbol(ITypeSymbol typeSymbol)
{
    public ITypeSymbol TypeSymbol => typeSymbol;

    public override int GetHashCode() => ToString().GetHashCode();

    public override bool Equals(object? obj) =>
        obj is EquatableTypeSymbol other
        && StringComparer.Ordinal.Equals(ToString(), other.ToString());

    public override string ToString() => TypeSymbol.ToDisplayString(FullyQualifiedFormat);

    public static bool operator ==(EquatableTypeSymbol? left, EquatableTypeSymbol? right) =>
        (object?)left == right || (left is not null && left.Equals(right));

    public static bool operator !=(EquatableTypeSymbol? left, EquatableTypeSymbol? right) =>
        !(left == right);
}

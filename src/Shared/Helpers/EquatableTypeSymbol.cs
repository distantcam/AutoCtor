using Microsoft.CodeAnalysis;

internal readonly record struct EquatableTypeSymbol(ITypeSymbol TypeSymbol)
{
    private readonly string _fullyQualifiedString = TypeSymbol.ToDisplayString(FullyQualifiedFormat);

    public override int GetHashCode() => ToString().GetHashCode();
    public bool Equals(EquatableTypeSymbol other) => EqualityComparer<string>.Default.Equals(ToString(), other.ToString());
    public override string ToString() => _fullyQualifiedString;
}

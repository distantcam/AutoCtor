using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

internal readonly record struct EquatableTypeSymbol(ITypeSymbol TypeSymbol)
{
    private static readonly ConditionalWeakTable<ITypeSymbol, string> s_displayStrings = new();

    private readonly string _fullyQualifiedString = s_displayStrings.GetValue(
        TypeSymbol, static t => t.ToDisplayString(FullyQualifiedFormat));

    public override int GetHashCode() => ToString().GetHashCode();
    public bool Equals(EquatableTypeSymbol other) => EqualityComparer<string>.Default.Equals(ToString(), other.ToString());
    public override string ToString() => _fullyQualifiedString;
}

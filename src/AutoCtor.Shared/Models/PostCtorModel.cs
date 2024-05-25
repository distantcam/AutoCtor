using Microsoft.CodeAnalysis;

internal record struct PostCtorModel(IMethodSymbol Method) : IEquatable<PostCtorModel>
{
    public override readonly int GetHashCode() =>
        Method is null ? 0 : Method.ToDisplayString().GetHashCode();
    public readonly bool Equals(PostCtorModel other) =>
        Method is null ? other.Method is null : Method.ToDisplayString().Equals(other.Method.ToDisplayString());
}

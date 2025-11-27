namespace Microsoft.CodeAnalysis;

internal static class Shim
{
    public static ISourceGenerator AsSourceGenerator(this ISourceGenerator generator) => generator;
}

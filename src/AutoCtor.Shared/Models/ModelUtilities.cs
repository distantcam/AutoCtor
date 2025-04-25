using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class ModelUtilities
{
    public static string? GetServiceKey(ISymbol symbol)
    {
        var keyedService = symbol.GetAttributes()
            .Where(a => a.AttributeClass?.ToDisplayString() == AttributeNames.AutoKeyedService
                || a.AttributeClass?.ToDisplayString() == "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute")
            .FirstOrDefault();

        if (keyedService != null)
            return keyedService.ConstructorArguments[0].ToCSharpString();

        return null;
    }
}

using Microsoft.CodeAnalysis;

internal readonly record struct DuckTypes(bool DI, bool Keyed, bool Async)
{
    public static DuckTypes Create(Compilation c) => new(
        c.GetTypeByMetadataName("Microsoft.Extensions.DependencyInjection.IServiceProviderIsService") is not null,
        c.GetTypeByMetadataName("Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider") is not null,
        c.GetTypeByMetadataName("System.IAsyncDisposable") is not null
            && c.GetTypeByMetadataName("System.Threading.Tasks.ValueTask") is not null);
}

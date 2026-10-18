internal static class AttributeNames
{
    public const string AutoCtorNamespace = "AutoCtor";

    public const string AutoConstructName = "AutoConstructAttribute";
    public const string AutoPostConstructName = "AutoPostConstructAttribute";
    public const string AutoConstructIgnoreName = "AutoConstructIgnoreAttribute";
    public const string AutoKeyedServiceName = "AutoKeyedServiceAttribute";
    public const string ServiceProviderName = "ServiceProviderAttribute";

    public const string AutoConstruct = $"{AutoCtorNamespace}.{AutoConstructName}";
    public const string AutoPostConstruct = $"{AutoCtorNamespace}.{AutoPostConstructName}";
    public const string AutoConstructIgnore = $"{AutoCtorNamespace}.{AutoConstructIgnoreName}";
    public const string AutoKeyedService = $"{AutoCtorNamespace}.{AutoKeyedServiceName}";
    public const string ServiceProvider = $"{AutoCtorNamespace}.{ServiceProviderName}";
}

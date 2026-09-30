using Microsoft.CodeAnalysis;

namespace AutoCtor;

internal static class Diagnostics
{
    /// <summary>
    /// Id: ACTR001<br />
    /// Title: Ambiguous marked post constructor method
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR001_AmbiguousMarkedPostConstructMethod = new DiagnosticDescriptor(
        id: "ACTR001",
        title: "Ambiguous marked post constructor method",
        messageFormat: "Only one method in a type should be marked with an [AutoPostConstruct] attribute",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR002<br />
    /// Title: Post construct method must return void
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR002_PostConstructMethodNotVoid = new DiagnosticDescriptor(
        id: "ACTR002",
        title: "Post construct method must return void",
        messageFormat: "The method '{0}' must return void to be used as the post construct method",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR003<br />
    /// Title: Post construct method must not have any optional arguments
    /// </summary>
    // Deprecated in 2.10
    // public static readonly DiagnosticDescriptor ACTR003_PostConstructMethodHasOptionalArgs = new DiagnosticDescriptor(
    //     id: "ACTR003",
    //     title: "Post construct method must not have any optional arguments",
    //     messageFormat: "The parameter '{0}' must not be optional",
    //     category: "AutoCtor",
    //     DiagnosticSeverity.Warning,
    //     isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR004<br />
    /// Title: Post construct method must not be generic
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR004_PostConstructMethodCannotBeGeneric = new DiagnosticDescriptor(
        id: "ACTR004",
        title: "Post construct method must not be generic",
        messageFormat: "The method '{0}' must not be generic to be used as the post construct method",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR005<br />
    /// Title: Post construct out or ref parameter must not be a keyed service
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR005_PostConstructOutParameterCannotBeKeyed = new DiagnosticDescriptor(
        id: "ACTR005",
        title: "Post construct out or ref parameter must not be a keyed service",
        messageFormat: "The parameter '{0}' must not be a keyed service, or cannot be out or ref",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR006<br />
    /// Title: Post construct out or ref parameter must not match a keyed field
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR006_PostConstructOutParameterMustNotMatchKeyedField = new DiagnosticDescriptor(
        id: "ACTR006",
        title: "Post construct out or ref parameter must not match a keyed field",
        messageFormat: "The field '{0}' must not be a keyed service when used as a post construct out or ref parameter",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR007<br />
    /// Title: Use [AutoConstruct] instead of manual constructor
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR007_UseAutoConstruct = new DiagnosticDescriptor(
        id: "ACTR007",
        title: "Use [AutoConstruct] instead of manual constructor",
        messageFormat: "Constructor for '{0}' can be replaced with [AutoConstruct]",
        category: "AutoCtor",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR008<br />
    /// Title: Add [AutoConstruct] to type
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR008_AddAutoConstruct = new DiagnosticDescriptor(
        id: "ACTR008",
        title: "Add [AutoConstruct] to type",
        messageFormat: "[AutoConstruct] can be added to type '{0}'",
        category: "AutoCtor",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR009<br />
    /// Title: Post construct out or ref parameter must match a field
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR009_PostConstructOutParameterMustMatchMember = new DiagnosticDescriptor(
        id: "ACTR009",
        title: "Post construct out or ref parameter must match a field",
        messageFormat: "The parameter '{0}' must match a field when used as a post construct out or ref parameter",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR010<br />
    /// Title: Service implementation must have a single public constructor
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR010_ServiceImplementationMustHaveSinglePublicConstructor = new DiagnosticDescriptor(
        id: "ACTR010",
        title: "Service implementation must have a single public constructor",
        messageFormat: "The implementation type '{0}' must have exactly one public constructor to be used as a service",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR012<br />
    /// Title: Service dependency is not registered
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR012_ServiceDependencyNotRegistered = new DiagnosticDescriptor(
        id: "ACTR012",
        title: "Service dependency is not registered",
        messageFormat: "No registration was found for the dependency '{0}' of '{1}'",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR013<br />
    /// Title: Circular dependency between registered services
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR013_CircularServiceDependency = new DiagnosticDescriptor(
        id: "ACTR013",
        title: "Circular dependency between registered services",
        messageFormat: "A circular dependency was detected involving '{0}'",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR014<br />
    /// Title: Keyed service dependency is not registered
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR014_KeyedServiceDependencyNotRegistered = new DiagnosticDescriptor(
        id: "ACTR014",
        title: "Keyed service dependency is not registered",
        messageFormat: "No registration with key {2} was found for the dependency '{0}' of '{1}'",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR018<br />
    /// Title: Transient service is captured by a singleton
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR018_TransientServiceCaptured = new DiagnosticDescriptor(
        id: "ACTR018",
        title: "Transient service is captured by a singleton",
        messageFormat: "The transient service '{0}' is captured by the singleton '{1}' and will not be recreated for each resolve",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR019<br />
    /// Title: Scoped service cannot be resolved outside a scope
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR019_ScopedServiceCapturedBySingleton = new DiagnosticDescriptor(
        id: "ACTR019",
        title: "Scoped service cannot be resolved outside a scope",
        messageFormat: "The service '{0}' requires a scope and cannot be injected into the singleton '{1}'",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR026<br />
    /// Title: Service scan found no types
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR026_ScanFoundNoTypes = new DiagnosticDescriptor(
        id: "ACTR026",
        title: "Service scan found no types",
        messageFormat: "The scan for '{0}' found no accessible, concrete, non-generic class to register",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR027<br />
    /// Title: Imported module has no registrations
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR027_ImportedModuleHasNoRegistrations = new DiagnosticDescriptor(
        id: "ACTR027",
        title: "Imported module has no registrations",
        messageFormat: "The module '{0}' has no registrations; a module in another assembly needs that assembly to define AUTOCTOR_USAGES",
        category: "AutoCtor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR011<br />
    /// Title: Service implementation cannot be instantiated
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR011_ServiceImplementationCannotBeInstantiated = new DiagnosticDescriptor(
        id: "ACTR011",
        title: "Service implementation cannot be instantiated",
        messageFormat: "The implementation type '{0}' cannot be instantiated; it must be a non-abstract, non-generic class",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR015<br />
    /// Title: Implementation type is not assignable to the service type
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR015_ImplementationNotAssignableToService = new DiagnosticDescriptor(
        id: "ACTR015",
        title: "Implementation type is not assignable to the service type",
        messageFormat: "The implementation type '{0}' is not assignable to the service type '{1}'",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR016<br />
    /// Title: Open generic registration is not valid
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR016_InvalidOpenGenericRegistration = new DiagnosticDescriptor(
        id: "ACTR016",
        title: "Open generic registration is not valid",
        messageFormat: "The open generic service '{0}' cannot be registered with '{1}'; the implementation must be an open generic type with the same number of type parameters",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR017<br />
    /// Title: Service provider type is not valid
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR017_InvalidServiceProviderType = new DiagnosticDescriptor(
        id: "ACTR017",
        title: "Service provider type is not valid",
        messageFormat: "The type '{0}' cannot be a service provider; it must not be generic, and must not be marked with [AutoConstruct]",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR020<br />
    /// Title: Type arguments do not satisfy the open generic implementation
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR020_OpenGenericConstraintNotSatisfied = new DiagnosticDescriptor(
        id: "ACTR020",
        title: "Type arguments do not satisfy the open generic implementation",
        messageFormat: "'{0}' is needed by '{1}', but its type arguments do not satisfy the constraints of '{2}'",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR021<br />
    /// Title: Service factory member is not valid
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR021_InvalidServiceFactory = new DiagnosticDescriptor(
        id: "ACTR021",
        title: "Service factory member is not valid",
        messageFormat: "The factory '{0}' cannot be used for the service '{1}'; it must name an accessible, non-generic method, property or field declared on the service provider that returns a value",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR022<br />
    /// Title: Service factory does not return the service type
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR022_ServiceFactoryReturnTypeNotAssignable = new DiagnosticDescriptor(
        id: "ACTR022",
        title: "Service factory does not return the service type",
        messageFormat: "The factory '{0}' returns '{1}', which is not assignable to the service type '{2}'",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR023<br />
    /// Title: Open generic registration cannot use a factory
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR023_OpenGenericServiceFactory = new DiagnosticDescriptor(
        id: "ACTR023",
        title: "Open generic registration cannot use a factory",
        messageFormat: "The open generic service '{0}' cannot use the factory '{1}'; a factory builds one type, and an open registration is closed on demand",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR024<br />
    /// Title: Service provider fallback member is not valid
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR024_InvalidProviderFallback = new DiagnosticDescriptor(
        id: "ACTR024",
        title: "Service provider fallback member is not valid",
        messageFormat: "The fallback '{0}' cannot be used by '{1}'; it must name an accessible, non-generic field, property or parameterless method declared on the service provider that returns a value",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR025<br />
    /// Title: Service provider fallback is not a service provider
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR025_ProviderFallbackNotAServiceProvider = new DiagnosticDescriptor(
        id: "ACTR025",
        title: "Service provider fallback is not a service provider",
        messageFormat: "The fallback '{0}' returns '{1}', which is not assignable to System.IServiceProvider",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR028<br />
    /// Title: Module factory must be static
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR028_ModuleFactoryMustBeStatic = new DiagnosticDescriptor(
        id: "ACTR028",
        title: "Module factory must be static",
        messageFormat: "The factory '{0}' on the module '{1}' must be static; a module is never instantiated",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Id: ACTR029<br />
    /// Title: Service provider type must be sealed
    /// </summary>
    public static readonly DiagnosticDescriptor ACTR029_ServiceProviderMustBeSealed = new DiagnosticDescriptor(
        id: "ACTR029",
        title: "Service provider type must be sealed",
        messageFormat: "The type '{0}' must be sealed to be a service provider",
        category: "AutoCtor",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

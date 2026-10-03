using System.Diagnostics;
using static System.AttributeTargets;

namespace AutoCtor;

/// <summary>
/// Generates a compile time <see cref="IServiceProvider"/> from the registration attributes on this class.
/// </summary>
[AttributeUsage(Class, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class ServiceProviderAttribute : Attribute
{
    /// <summary>
    /// A field, property or parameterless method (named with <c>nameof</c>) returning an
    /// <see cref="IServiceProvider"/> to ask for anything not registered.
    /// </summary>
    public string? Fallback { get; set; }
}

public abstract class ServiceAttribute : Attribute
{
    public object? Key { get; set; }

    /// <summary>
    /// A method, property or field (named with <c>nameof</c>) that builds the service instead
    /// of its constructor. A method's parameters are resolved like constructor parameters.
    /// </summary>
    public string? Factory { get; set; }
}

[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class SingletonAttribute(Type service, Type? implementation = null) : ServiceAttribute
{
    public Type Service { get; } = service;
    public Type? Implementation { get; } = implementation;
}

[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class SingletonAttribute<TService> : ServiceAttribute;

[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class SingletonAttribute<TService, TImplementation> : ServiceAttribute;

[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class TransientAttribute(Type service, Type? implementation = null) : ServiceAttribute
{
    public Type Service { get; } = service;
    public Type? Implementation { get; } = implementation;
}

[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class TransientAttribute<TService> : ServiceAttribute;

[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class TransientAttribute<TService, TImplementation> : ServiceAttribute;

[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class ScopedAttribute(Type service, Type? implementation = null) : ServiceAttribute
{
    public Type Service { get; } = service;
    public Type? Implementation { get; } = implementation;
}

[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class ScopedAttribute<TService> : ServiceAttribute;

[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class ScopedAttribute<TService, TImplementation> : ServiceAttribute;

/// <summary>
/// Adds every registration, scan and import on another class, in place. A module in another
/// assembly is only seen when that assembly defines <c>AUTOCTOR_USAGES</c>.
/// </summary>
[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class ImportAttribute(Type module) : Attribute
{
    public Type Module { get; } = module;
}

/// <inheritdoc cref="ImportAttribute"/>
[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class ImportAttribute<TModule> : Attribute;

[Flags]
public enum ScanAs
{
    /// <summary>The scanned for type, closed over the match for an open generic.</summary>
    Service = 1,
    /// <summary>The implementation itself.</summary>
    Self = 2,
    /// <summary>Every interface of the implementation.</summary>
    ImplementedInterfaces = 4,
}

/// <summary>
/// Registers every accessible, concrete, non-generic class assignable to <see cref="Service"/>,
/// and matching <see cref="TypeNameFilter"/>. At least one of the two is required.
/// </summary>
public abstract class ScanAttribute(Type? service) : Attribute
{
    /// <summary>
    /// An open generic such as <c>typeof(IHandler&lt;&gt;)</c> matches every construction of it.
    /// Without one, <see cref="ScanAs.Service"/> registers each type as itself.
    /// </summary>
    public Type? Service { get; } = service;
    public ScanAs As { get; set; } = ScanAs.Service;
    /// <summary>Scans the assemblies of these types instead of the provider's own.</summary>
    public Type[]? FromAssembliesOf { get; set; }
    /// <summary>Only types whose name matches, where <c>*</c> matches anything and <c>?</c> any one character, such as <c>"*Repository"</c>.</summary>
    public string? TypeNameFilter { get; set; }
}

[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class ScanSingletonAttribute(Type? service = null) : ScanAttribute(service);

[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class ScanTransientAttribute(Type? service = null) : ScanAttribute(service);

[AttributeUsage(Class, AllowMultiple = true, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class ScanScopedAttribute(Type? service = null) : ScanAttribute(service);

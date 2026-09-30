using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static AutoCtor.Diagnostics;

internal enum Lifetime { Singleton, Scoped, Transient }

internal readonly record struct RegistrationModel(
    Lifetime Lifetime,
    EquatableTypeSymbol Service,
    EquatableTypeSymbol Implementation,
    // Both as C# source, the key comparable with ParameterModel.KeyedService.
    string? Key,
    // "{root}" is replaced by the container the factory is called from.
    string? Factory,
    bool FactoryIsMethod,
    bool IsOpenGeneric,
    bool IsAutoConstruct,
    // Decide the cheapest way to cache and to dispose it.
    bool ServiceIsReferenceType,
    bool ImplementationIsDisposable,
    int PublicConstructorCount,
    EquatableList<ParameterModel> Parameters,
    string ErrorName,
    EquatableList<Location> Locations
) : IHaveDiagnostics;

// Found while reading the attributes, where the symbols are at hand.
internal readonly record struct ModelDiagnostic(DiagnosticDescriptor Descriptor, EquatableList<string> Args, EquatableList<Location> Locations) : IHaveDiagnostics
{
    public string ErrorName => Args[0];
}

internal readonly record struct ServiceProviderModel(
    string? Namespace,
    string Name,
    string HintName,
    EquatableList<string> TypeDeclarations,
    string? Fallback,
    EquatableList<RegistrationModel> Registrations,
    EquatableList<ModelDiagnostic> Diagnostics
) : IPartialTypeModel
{
    public static ServiceProviderModel Create(INamedTypeSymbol type)
    {
        var registrations = new List<RegistrationModel>();
        var diagnostics = new List<ModelDiagnostic>();
        AddRegistrations(type, type, new(SymbolEqualityComparer.Default) { type }, [], registrations, diagnostics);

        if (type.IsGenericType || Utilities.HasAttribute(type, AttributeNames.AutoConstruct))
            Report(diagnostics, ACTR017_InvalidServiceProviderType, type.Locations, DisplayName(type));

        if (!type.IsSealed)
            Report(diagnostics, ACTR029_ServiceProviderMustBeSealed, type.Locations, DisplayName(type));

        var fallback = type.GetAttributes()
            .First(a => a.AttributeClass?.ToDisplayString() == AttributeNames.ServiceProvider)
            .NamedArguments.FirstOrDefault(n => n.Key == "Fallback").Value.Value as string;
        string? fallbackAccess = null;
        if (fallback is not null)
        {
            fallbackAccess = MemberAccess(type, fallback, out var member) + (member is IMethodSymbol ? "()" : "");
            var fallbackType = MemberType(type, member, allowParameters: false);
            if (fallbackType is null)
                Report(diagnostics, ACTR024_InvalidProviderFallback, type.Locations, fallback, DisplayName(type));
            else if (!fallbackType.AllInterfaces.Prepend(fallbackType).Any(i => i.ToDisplayString() == "System.IServiceProvider"))
                Report(diagnostics, ACTR025_ProviderFallbackNotAServiceProvider, type.Locations, fallback, DisplayName(fallbackType));
        }

        return new(
            Namespace: GeneratorUtilities.GetNamespace(type),
            Name: type.Name,
            HintName: GeneratorUtilities.GetHintName(type),
            TypeDeclarations: GeneratorUtilities.GetTypeDeclarations(type),
            Fallback: fallbackAccess,
            Registrations: new(registrations),
            Diagnostics: new(diagnostics));
    }

    // Walks the attributes in order, expanding scans and imports in place, so a later
    // registration replaces an earlier one wherever it came from.
    private static void AddRegistrations(
        INamedTypeSymbol provider,
        INamedTypeSymbol source,
        HashSet<INamedTypeSymbol> visited,
        Location[] importLocations,
        List<RegistrationModel> registrations,
        List<ModelDiagnostic> diagnostics)
    {
        // Syntax from another assembly can't be reported against, so use the import instead.
        var ownSyntax = SymbolEqualityComparer.Default.Equals(source.ContainingAssembly, provider.ContainingAssembly);

        foreach (var attribute in source.GetAttributes())
        {
            if (attribute.AttributeClass is not { ContainingNamespace.Name: "AutoCtor" } attributeClass)
                continue;

            var name = attributeClass.Name;
            var typeArgs = attributeClass.TypeArguments;
            var ctorArgs = attribute.ConstructorArguments;
            var locations = ownSyntax && attribute.ApplicationSyntaxReference is { } r
                ? [Location.Create(r.SyntaxTree, r.Span)]
                : importLocations;
            var lifetime = name.Contains("Scoped") ? Lifetime.Scoped
                : name.Contains("Transient") ? Lifetime.Transient
                : Lifetime.Singleton;
            var before = registrations.Count;

            if (name == "ImportAttribute")
            {
                // Visited once, so cycles end and a module imported twice isn't doubled up.
                if ((typeArgs.Length > 0 ? typeArgs[0] : ctorArgs[0].Value) is INamedTypeSymbol module
                    && visited.Add(module))
                {
                    var diagnosticsBefore = diagnostics.Count;
                    AddRegistrations(provider, module, visited, locations, registrations, diagnostics);
                    // An empty scan or a bad registration inside already says why.
                    if (registrations.Count == before && diagnostics.Count == diagnosticsBefore)
                        Report(diagnostics, ACTR027_ImportedModuleHasNoRegistrations, locations, DisplayName(module));
                }
            }
            else if (name.StartsWith("Scan", StringComparison.Ordinal))
            {
                if (ctorArgs[0].Value is not INamedTypeSymbol filter)
                    continue;

                foreach (var (service, implementation) in Scan(provider, source, attribute, filter))
                    Add(provider, source, lifetime, service, implementation, null, null, locations, registrations, diagnostics);

                if (registrations.Count == before)
                    Report(diagnostics, ACTR026_ScanFoundNoTypes, locations, DisplayName(filter));
            }
            else if (name is "SingletonAttribute" or "TransientAttribute" or "ScopedAttribute")
            {
                var service = typeArgs.Length > 0 ? typeArgs[0] : ctorArgs[0].Value as ITypeSymbol;
                var implementation = (typeArgs.Length > 1
                    ? typeArgs[1]
                    : ctorArgs.Length > 1
                        ? ctorArgs[1].Value as ITypeSymbol
                        : null)
                    ?? service;
                var key = attribute.NamedArguments
                    .Where(n => n.Key == "Key" && !n.Value.IsNull)
                    .Select(n => n.Value.ToCSharpString())
                    .FirstOrDefault();
                var factory = attribute.NamedArguments
                    .FirstOrDefault(n => n.Key == "Factory").Value.Value as string;
                Add(provider, source, lifetime, service, implementation, key, factory, locations, registrations, diagnostics);
            }
        }
    }

    private static void Add(
        INamedTypeSymbol provider,
        INamedTypeSymbol source,
        Lifetime lifetime,
        ITypeSymbol? service,
        ITypeSymbol? implementation,
        string? key,
        string? factory,
        Location[] locations,
        List<RegistrationModel> registrations,
        List<ModelDiagnostic> diagnostics)
    {
        if (service is null or IErrorTypeSymbol || implementation is not INamedTypeSymbol impl || impl is IErrorTypeSymbol)
            return;

        var isOpen = (service as INamedTypeSymbol)?.IsUnboundGenericType == true;
        if (isOpen)
        {
            service = service.OriginalDefinition;
            impl = impl.OriginalDefinition;
        }

        var ctors = impl.InstanceConstructors
            .Where(c => c.DeclaredAccessibility == Accessibility.Public)
            .ToList();
        var parameters = ctors.Count == 1 ? ctors[0].Parameters : ImmutableArray<IParameterSymbol>.Empty;

        ISymbol? member = null;
        var factoryAccess = factory is null ? null : MemberAccess(source, factory, out member);
        if (factory is not null)
            parameters = member is IMethodSymbol method ? method.Parameters : ImmutableArray<IParameterSymbol>.Empty;

        var (serviceName, implName) = (DisplayName(service), DisplayName(impl));
        var factoryType = MemberType(provider, member, allowParameters: true);
        (DiagnosticDescriptor, string[])? error =
            isOpen && factory is not null ? (ACTR023_OpenGenericServiceFactory, [serviceName, factory])
            : isOpen && impl.Arity != ((INamedTypeSymbol)service).Arity ? (ACTR016_InvalidOpenGenericRegistration, [serviceName, implName])
            : !IsAssignable(impl, service, isOpen) ? (ACTR015_ImplementationNotAssignableToService, [implName, serviceName])
            : factory is null && (impl.IsAbstract || impl.IsStatic || impl.TypeKind is not (TypeKind.Class or TypeKind.Struct)
                || !isOpen && impl.TypeArguments.Any(t => t is ITypeParameterSymbol)) ? (ACTR011_ServiceImplementationCannotBeInstantiated, [implName])
            : factory is null ? null
            : factoryType is null ? (ACTR021_InvalidServiceFactory, [factory, serviceName])
            : !IsAssignable(factoryType, service) ? (ACTR022_ServiceFactoryReturnTypeNotAssignable, [factory, DisplayName(factoryType), serviceName])
            : member is { IsStatic: false } && !SymbolEqualityComparer.Default.Equals(source, provider) ? (ACTR028_ModuleFactoryMustBeStatic, [factory, DisplayName(source)])
            : null;
        if (error is var (descriptor, args))
        {
            Report(diagnostics, descriptor, locations, args);
            return;
        }

        registrations.Add(new(
            Lifetime: lifetime,
            Service: new(service),
            Implementation: new(impl),
            Key: key,
            Factory: factoryAccess,
            FactoryIsMethod: member is IMethodSymbol,
            IsOpenGeneric: isOpen,
            IsAutoConstruct: Utilities.HasAttribute(impl, AttributeNames.AutoConstruct),
            ServiceIsReferenceType: service.IsReferenceType,
            // A factory can hand back anything.
            ImplementationIsDisposable: factory is not null || impl.AllInterfaces.Any(i => i.ToDisplayString() is "System.IDisposable" or "System.IAsyncDisposable"),
            PublicConstructorCount: ctors.Count,
            Parameters: new(parameters.Select(ParameterModel.Create)),
            ErrorName: impl.ToDisplayString(MinimallyQualifiedFormat),
            Locations: new(locations)));
    }

    private static string DisplayName(ITypeSymbol type) => type.ToDisplayString(MinimallyQualifiedFormat);

    private static void Report(List<ModelDiagnostic> diagnostics, DiagnosticDescriptor descriptor, IEnumerable<Location> locations, params string[] args)
        => diagnostics.Add(new(descriptor, new(args), new(locations)));

    // The type, an interface it implements, or a base class. An open registration compares definitions.
    internal static bool IsAssignable(ITypeSymbol type, ITypeSymbol target, bool byDefinition = false)
        => target.SpecialType == SpecialType.System_Object
        || type.AllInterfaces.Concat(type is INamedTypeSymbol named ? BaseTypes(named) : []).Prepend<ITypeSymbol>(type)
            .Any(t => SymbolEqualityComparer.Default.Equals(byDefinition ? t.OriginalDefinition : t, byDefinition ? target.OriginalDefinition : target));

    // What a named member produces, or null when generated code can't use it: not a value,
    // generic, or out of reach from the provider.
    private static ITypeSymbol? MemberType(INamedTypeSymbol provider, ISymbol? member, bool allowParameters)
    {
        if (member is null || member.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal
            && !SymbolEqualityComparer.Default.Equals(member.ContainingType, provider))
            return null;

        var type = member switch
        {
            IMethodSymbol { MethodKind: MethodKind.Ordinary, IsGenericMethod: false, ReturnsVoid: false, RefKind: RefKind.None } m
                when (allowParameters ? m.Parameters.All(p => p.RefKind == RefKind.None) : m.Parameters.Length == 0) => m.ReturnType,
            IPropertySymbol { IsIndexer: false, GetMethod: not null } p => p.Type,
            IFieldSymbol f => f.Type,
            _ => null,
        };
        return type is IErrorTypeSymbol ? null : type;
    }

    // Static members are called through their type, instance members through the provider.
    private static string MemberAccess(INamedTypeSymbol owner, string name, out ISymbol? member)
    {
        member = null;
        for (var t = owner; t is not null && member is null; t = t.BaseType)
            member = t.GetMembers(name).FirstOrDefault();

        return member is { IsStatic: true }
            ? $"{member.ContainingType.ToDisplayString(FullyQualifiedFormat)}.{name}"
            : $"{{root}}.{name}";
    }

    // ponytail: walks every type in the scanned assemblies on each run; index by interface if it shows up.
    private static IEnumerable<(ITypeSymbol, INamedTypeSymbol)> Scan(
        INamedTypeSymbol provider, INamedTypeSymbol source, AttributeData attribute, INamedTypeSymbol filter)
    {
        var scanAs = 1;
        var assemblies = new List<IAssemblySymbol>();
        foreach (var named in attribute.NamedArguments)
        {
            if (named.Key == "As" && named.Value.Value is int flags)
                scanAs = flags;
            else if (named.Key == "FromAssembliesOf" && !named.Value.IsNull)
                assemblies.AddRange(named.Value.Values.Select(v => (v.Value as ITypeSymbol)?.ContainingAssembly).OfType<IAssemblySymbol>());
        }
        if (assemblies.Count == 0)
            assemblies.Add(source.ContainingAssembly);

        var isOpen = filter.IsUnboundGenericType;
        var target = isOpen ? filter.OriginalDefinition : filter;

        var candidates = assemblies
            .Distinct<IAssemblySymbol>(SymbolEqualityComparer.Default)
            .SelectMany(a => VisibleTypes(a.GlobalNamespace,
                SymbolEqualityComparer.Default.Equals(a, provider.ContainingAssembly) || a.GivesAccessTo(provider.ContainingAssembly)))
            .Where(t => t is { TypeKind: TypeKind.Class, IsAbstract: false, IsStatic: false, IsGenericType: false }
                && !SymbolEqualityComparer.Default.Equals(t, provider))
            // Neither source nor metadata promise an order, and order decides which registration wins.
            .OrderBy(t => t.ToDisplayString(FullyQualifiedFormat), StringComparer.Ordinal);

        foreach (var type in candidates)
        {
            var matches = type.AllInterfaces.Concat(BaseTypes(type)).Prepend(type)
                .Where(t => SymbolEqualityComparer.Default.Equals(isOpen ? t.OriginalDefinition : t, target))
                .ToList<ITypeSymbol>();
            if (matches.Count == 0)
                continue;

            var services = new List<ITypeSymbol>();
            if ((scanAs & 1) != 0) services.AddRange(matches);
            if ((scanAs & 2) != 0) services.Add(type);
            if ((scanAs & 4) != 0) services.AddRange(type.AllInterfaces);

            foreach (var service in services.Distinct<ITypeSymbol>(SymbolEqualityComparer.Default))
                yield return (service, type);
        }
    }

    private static IEnumerable<INamedTypeSymbol> BaseTypes(INamedTypeSymbol type)
    {
        for (var b = type.BaseType; b is not null; b = b.BaseType)
            yield return b;
    }

    // Every type in a namespace, and nested in those types, that the provider's assembly can name.
    private static IEnumerable<INamedTypeSymbol> VisibleTypes(INamespaceOrTypeSymbol container, bool internals) =>
        (container is INamespaceSymbol ns ? ns.GetNamespaceMembers().SelectMany(n => VisibleTypes(n, internals)) : [])
        .Concat(container.GetTypeMembers()
            .Where(t => t.DeclaredAccessibility == Accessibility.Public
                || internals && t.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedOrInternal)
            .SelectMany(t => VisibleTypes(t, internals).Prepend(t)));
}

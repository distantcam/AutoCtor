using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

internal readonly record struct TypeModel(
    string TypeKey,

    string? Namespace,
    string Name,

    bool HasBaseType,

    string HintName,

    bool? Guard,

    EquatableList<string> TypeDeclarations,
    EquatableList<MemberModel> Fields,
    EquatableList<MemberModel> Properties,
    EquatableList<ParameterModel>? BaseCtorParameters,
    EquatableList<EquatableTypeSymbol>? BaseTypeArguments,
    EquatableList<EquatableTypeSymbol>? BaseTypeParameters,
    EquatableList<PostCtorModel> PostCtorMethods,
    EquatableList<TypeModel> AutoConstructBase
) : IPartialTypeModel
{
    public static TypeModel Create(INamedTypeSymbol type)
    {
        var attribute = Utilities.FindAttribute(type, AttributeNames.AutoConstructName);

        var guard = ((int?)attribute?.ConstructorArguments[0].Value) switch
        {
            1 => false, // GuardSetting.Disabled
            2 => true, // GuardSetting.Enabled
            _ => (bool?)null,
        };

        var baseCtorParameters = type.BaseType?.Constructors.OnlyOrDefault(ValidCtor)?.Parameters;
        var genericBaseType = type.BaseType is { IsGenericType: true };

        var members = type.GetMembers();

        var autoConstructBase = baseCtorParameters == null
            && type.BaseType?.OriginalDefinition is { } baseType
            && SymbolEqualityComparer.Default.Equals(baseType.ContainingAssembly, type.ContainingAssembly)
            && Utilities.HasAttribute(baseType, AttributeNames.AutoConstructName)
            ? new EquatableList<TypeModel>(ImmutableArray.Create(Create(baseType)))
            : new EquatableList<TypeModel>(ImmutableArray<TypeModel>.Empty);

        return new(
            TypeKey: CreateKey(type),

            Namespace: GeneratorUtilities.GetNamespace(type),
            Name: type.Name,

            HasBaseType: type.BaseType is not null,

            HintName: GeneratorUtilities.GetHintName(type),

            Guard: guard,

            TypeDeclarations: GeneratorUtilities.GetTypeDeclarations(type),

            Fields: GetFields(members),
            Properties: GetProperties(members),

            BaseCtorParameters: baseCtorParameters != null
                ? new(baseCtorParameters.Value.Select(ParameterModel.Create))
                : null,

            BaseTypeArguments: genericBaseType
                ? new(type.BaseType!.TypeArguments.Select(Create))
                : null,
            BaseTypeParameters: genericBaseType
                ? new(type.BaseType!.TypeParameters.Select(Create))
                : null,

            PostCtorMethods: GetPostCtorMethods(members),
            AutoConstructBase: autoConstructBase
        );
    }

    private static EquatableTypeSymbol Create(ITypeSymbol type) => new(type);

    private static bool ValidCtor(IMethodSymbol ctor)
    {
        if (ctor.IsStatic)
            return false;

        if (!ctor.Parameters.Any())
            return false;

        if (ctor.GetAttributes()
            .Any(static a => GeneratorUtilities.IsType(a.AttributeClass, "System", "ObsoleteAttribute")))
            return false;

        // Don't use the record clone ctor
        if (ctor.ContainingType.IsRecord &&
            ctor is { DeclaredAccessibility: Accessibility.Protected, Parameters.Length: 1 } &&
            SymbolEqualityComparer.Default.Equals(ctor.Parameters[0].Type, ctor.ContainingType))
        {
            return false;
        }

        return true;
    }

    public static string CreateKey(INamedTypeSymbol? type)
    {
        if (type is null)
            return string.Empty;

        if (type.IsGenericType)
            return type.ConstructUnboundGenericType().ToDisplayString(FullyQualifiedFormat);

        return type.ToDisplayString(FullyQualifiedFormat);
    }

    private static EquatableList<MemberModel> GetFields(ImmutableArray<ISymbol> members)
    {
        var fields = new List<IFieldSymbol>();
        foreach (var member in members)
        {
            if (member is IFieldSymbol field && Utilities.IsValidField(field))
                fields.Add(field);
        }
        var builder = ImmutableArray.CreateBuilder<MemberModel>(fields.Count);
        foreach (var field in fields)
            builder.Add(MemberModel.Create(field));
        return new(builder.MoveToImmutable());
    }

    private static EquatableList<PostCtorModel> GetPostCtorMethods(ImmutableArray<ISymbol> members)
    {
        var builder = ImmutableArray.CreateBuilder<PostCtorModel>();
        foreach (var member in members)
        {
            if (member is IMethodSymbol method && Utilities.HasAttribute(method, AttributeNames.AutoPostConstructName))
                builder.Add(PostCtorModel.Create(method));
        }
        return new(builder.ToImmutable());
    }

    private static EquatableList<MemberModel> GetProperties(ImmutableArray<ISymbol> members)
    {
        var properties = new List<IPropertySymbol>();
        foreach (var member in members)
        {
            if (member is IPropertySymbol property && Utilities.IsValidProperty(property))
                properties.Add(property);
        }
        var builder = ImmutableArray.CreateBuilder<MemberModel>(properties.Count);
        foreach (var property in properties)
            builder.Add(MemberModel.Create(property));
        return new(builder.MoveToImmutable());
    }
}

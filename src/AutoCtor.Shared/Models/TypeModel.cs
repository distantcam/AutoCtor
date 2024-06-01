using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal record struct TypeModel(
    int Depth,

    string BaseTypeKey,
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
    EquatableList<string>? BaseTypeArguments,
    EquatableList<string>? BaseTypeParameters
) : IPartialTypeModel
{
    readonly IReadOnlyList<string> IPartialTypeModel.TypeDeclarations => TypeDeclarations;

    public static TypeModel Create(INamedTypeSymbol type, AttributeData? attribute = null)
    {
        attribute ??= type.GetAttributes()
            .First(a => a.AttributeClass?.ToDisplayString() == "AutoCtor.AutoConstructAttribute");

        var guard = ((int?)attribute?.ConstructorArguments[0].Value) switch
        {
            1 => false, // GuardSetting.Disabled
            2 => true, // GuardSetting.Enabled
            _ => (bool?)null,
        };

        var baseCtorParameters = type.BaseType?.Constructors.OnlyOrDefault(ValidCtor)?.Parameters;
        var genericBaseType = type.BaseType is { IsGenericType: true };

        return new(
            Depth: CalculateInheritanceDepth(type),

            BaseTypeKey: CreateKey(type.BaseType),
            TypeKey: CreateKey(type),

            Namespace: GeneratorUtilities.GetNamespace(type),
            Name: type.Name,

            HasBaseType: type.BaseType is not null,

            HintName: GeneratorUtilities.GetHintName(type),

            Guard: guard,

            TypeDeclarations: GeneratorUtilities.GetTypeDeclarations(type),

            Fields: new(type.GetMembers().OfType<IFieldSymbol>()
                .Where(f => f is
                {
                    IsReadOnly: true,
                    IsStatic: false,
                    CanBeReferencedByName: true,
                    IsImplicitlyDeclared: false
                } && IsValidField(f))
                .Select(MemberModel.Create)),

            Properties: new(type.GetMembers().OfType<IPropertySymbol>()
                .Where(p => p is
                {
                    IsStatic: false,
                    CanBeReferencedByName: true,
                    IsImplicitlyDeclared: false,
                } && IsValidProperty(p))
                .Select(MemberModel.Create)),

            BaseCtorParameters: baseCtorParameters != null
                ? new(baseCtorParameters.Value.Select(ParameterModel.Create))
                : null,

            BaseTypeArguments: genericBaseType
                ? new(type.BaseType!.TypeArguments.Select(t => t.ToDisplayString(FullyQualifiedFormat)))
                : null,
            BaseTypeParameters: genericBaseType
                ? new(type.BaseType!.TypeParameters.Select(t => t.ToDisplayString(FullyQualifiedFormat)))
                : null
        );
    }

    private static bool ValidCtor(IMethodSymbol ctor)
    {
        if (ctor.IsStatic)
            return false;

        if (!ctor.Parameters.Any())
            return false;

        if (ctor.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString(FullyQualifiedFormat) == "global::System.ObsoleteAttribute"))
            return false;

        // Don't use the record clone ctor
        if (ctor.ContainingType.IsRecord &&
            ctor is { DeclaredAccessibility: Accessibility.Protected, Parameters.Length: 1 } &&
            SymbolEqualityComparer.Default.Equals(ctor.Parameters[0].Type, ctor.ContainingType))
            return false;

        return true;
    }

    private static int CalculateInheritanceDepth(ITypeSymbol type)
    {
        var depth = 0;
        var b = type.BaseType;
        while (b != null)
        {
            depth++;
            b = b.BaseType;
        }
        return depth;
    }

    public static string CreateKey(INamedTypeSymbol? type)
    {
        if (type is null)
            return string.Empty;

        if (type.IsGenericType)
            return type.ConstructUnboundGenericType().ToDisplayString(FullyQualifiedFormat);

        return type.ToDisplayString(FullyQualifiedFormat);
    }

    private static bool IsValidField(IFieldSymbol field)
    {
        if (field.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<VariableDeclaratorSyntax>()
            .Any(x => x.Initializer != null))
            return false;

        if (HasIgnoreAttribute(field.GetAttributes()))
            return false;

        return true;
    }

    private static bool IsValidProperty(IPropertySymbol property)
    {
        if (!property.ContainingType.GetMembers().OfType<IFieldSymbol>()
            .Any(field => SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, property)))
            return false;

        var propertySyntax = property.DeclaringSyntaxReferences
            .Select(x => x.GetSyntax())
            .OfType<PropertyDeclarationSyntax>()
            .First();

        if (propertySyntax.Initializer is not null)
            return false;

        if (!(property.IsReadOnly ||
#if ROSLYN_4_4
            property.IsRequired ||
#endif
            propertySyntax.AccessorList?.Accessors
            .Any(a => a.Kind() == SyntaxKind.InitAccessorDeclaration) == true))
            return false;

        if (HasIgnoreAttribute(property.GetAttributes()))
            return false;

        return true;
    }

    private static bool HasIgnoreAttribute(IEnumerable<AttributeData> attributes) =>
        attributes.Any(a => a.AttributeClass?.ToDisplayString() == "AutoCtor.AutoConstructIgnoreAttribute");
}

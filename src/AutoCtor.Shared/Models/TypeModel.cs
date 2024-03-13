using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.SymbolDisplayFormat;

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
    EquatableList<IFieldSymbol> Fields,
    EquatableList<IParameterSymbol>? BaseCtorParameters,
    EquatableList<ITypeSymbol>? BaseTypeArguments,
    EquatableList<ITypeParameterSymbol>? BaseTypeParameters
) : IPartialTypeModel
{
    readonly IReadOnlyList<string> IPartialTypeModel.TypeDeclarations => TypeDeclarations;

    public static TypeModel Create(INamedTypeSymbol type, AttributeData? attribute = null)
    {
        attribute ??= type.GetAttributes()
            .First(a => a.AttributeClass?.ToDisplayString() == "AutoCtor.AutoConstructAttribute");

        var guard = ((int?)attribute?.ConstructorArguments[0].Value) switch
        {
            1 => false,
            2 => true,
            _ => (bool?)null,
        };

        var baseCtorParameters = type.BaseType?.Constructors.OnlyOrDefault(ValidCtor)?.Parameters;
        var genericBaseType = type.BaseType != null && type.BaseType.IsGenericType;

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
                .Where(f => f.IsReadOnly && !f.IsStatic && f.CanBeReferencedByName && !HasFieldInitialiser(f))),

            BaseCtorParameters: baseCtorParameters != null ? new(baseCtorParameters) : null,

            BaseTypeArguments: genericBaseType ? new(type.BaseType!.TypeArguments) : null,
            BaseTypeParameters: genericBaseType ? new(type.BaseType!.TypeParameters) : null
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
        if (ctor.ContainingType.IsRecord && ctor.DeclaredAccessibility == Accessibility.Protected && ctor.Parameters.Length == 1 && SymbolEqualityComparer.Default.Equals(ctor.Parameters[0].Type, ctor.ContainingType))
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
            return type.ConstructUnboundGenericType().ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static bool HasFieldInitialiser(IFieldSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<VariableDeclaratorSyntax>().Any(x => x.Initializer != null);
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoCtor.Models;

internal record struct TypeModel(
    int Depth,

    string BaseTypeKey,
    string TypeKey,

    string? Namespace,
    string Name,

    bool HasBaseType,

    string HintName,

    EquatableList<string> TypeDeclarations,
    EquatableList<IFieldSymbol> Fields,
    EquatableList<IParameterSymbol>? BaseCtorParameters,
    EquatableList<ITypeSymbol>? BaseTypeArguments,
    EquatableList<ITypeParameterSymbol>? BaseTypeParameters
)
{
    public static TypeModel Create(INamedTypeSymbol type)
    {
        var baseCtorParameters = type.BaseType?.Constructors
                .OnlyOrDefault(c => !c.IsStatic && c.Parameters.Any())?.Parameters;
        var genericBaseType = type.BaseType != null && type.BaseType.IsGenericType;

        return new(
            Depth: CalculateInheritanceDepth(type),

            BaseTypeKey: CreateKey(type.BaseType),
            TypeKey: CreateKey(type),

            Namespace: type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace.ToString(),
            Name: type.Name,

            HasBaseType: type.BaseType is not null,

            HintName: CreateHintName(type),

            TypeDeclarations: CreateTypeDeclarations(type),

            Fields: new EquatableList<IFieldSymbol>(type.GetMembers().OfType<IFieldSymbol>()
                .Where(f => f.IsReadOnly && !f.IsStatic && f.CanBeReferencedByName && !HasFieldInitialiser(f))),

            BaseCtorParameters: baseCtorParameters != null ? new EquatableList<IParameterSymbol>(baseCtorParameters) : null,

            BaseTypeArguments: genericBaseType ? new EquatableList<ITypeSymbol>(type.BaseType!.TypeArguments) : null,
            BaseTypeParameters: genericBaseType ? new EquatableList<ITypeParameterSymbol>(type.BaseType!.TypeParameters) : null
        );
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
    private static string CreateHintName(ITypeSymbol type)
    {
        return type.ToDisplayString(GeneratorUtilities.HintSymbolDisplayFormat)
            .Replace('<', '[')
            .Replace('>', ']');
    }
    private static EquatableList<string> CreateTypeDeclarations(ITypeSymbol type)
    {
        var typeDeclarations = new List<string>();
        var currentType = type;
        while (currentType is not null)
        {
            var typeKeyword = currentType switch
            {
                { IsRecord: true, IsValueType: true } => "record struct",
                { IsRecord: true, IsValueType: false } => "record",
                { IsRecord: false, IsValueType: true } => "struct",
                { IsRecord: false, IsValueType: false } => "class",
                _ => string.Empty
            };

            var typeName = currentType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            typeDeclarations.Add($"partial {typeKeyword} {typeName}");
            currentType = currentType.ContainingType;
        }
        typeDeclarations.Reverse();
        return new EquatableList<string>(typeDeclarations);
    }

    private static bool HasFieldInitialiser(IFieldSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<VariableDeclaratorSyntax>().Any(x => x.Initializer != null);
    }
}

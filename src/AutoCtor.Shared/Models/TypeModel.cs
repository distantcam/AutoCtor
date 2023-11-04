using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoCtor.Models;

internal record TypeData(
    int Depth,

    string BaseTypeKey,
    string TypeKey,

    string? Namespace,
    string Name,

    bool HasBaseType,

    string HintName
);

internal class TypeModel : IEquatable<TypeModel>
{
    public TypeData Data { get; }

    public IReadOnlyList<string> TypeDeclarations { get; }
    public IReadOnlyList<IFieldSymbol> Fields { get; }
    public IReadOnlyList<IParameterSymbol>? BaseCtorParameters { get; }
    public IReadOnlyList<ITypeSymbol>? BaseTypeArguments { get; }
    public IReadOnlyList<ITypeParameterSymbol>? BaseTypeParameters { get; }

    public TypeModel(INamedTypeSymbol type)
    {
        Data = new TypeData(
            CalculateInheritanceDepth(type),

            CreateKey(type.BaseType),
            CreateKey(type),

            type.ContainingNamespace.IsGlobalNamespace ? null : type.ContainingNamespace.ToString(),
            type.Name,

            type.BaseType is not null,

            CreateHintName(type)
        );

        Fields = type.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f.IsReadOnly && !f.IsStatic && f.CanBeReferencedByName && !HasFieldInitialiser(f))
            .ToList();

        if (type.BaseType != null && type.BaseType.IsGenericType)
        {
            BaseTypeArguments = type.BaseType.TypeArguments;
            BaseTypeParameters = type.BaseType.TypeParameters;
        }
        BaseCtorParameters = type.BaseType?.Constructors
            .OnlyOrDefault(c => !c.IsStatic && c.Parameters.Any())?.Parameters;

        TypeDeclarations = CreateTypeDeclarations(type);
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
    private static IReadOnlyList<string> CreateTypeDeclarations(ITypeSymbol type)
    {
        var typeDeclarations = new List<string>();
        var currentType = type;
        while (currentType is not null)
        {
            var typeKeyword = currentType.IsRecord
                ? "record"
                : currentType.IsValueType
                    ? "struct"
                    : "class";
            var typeName = currentType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            typeDeclarations.Add($"partial {typeKeyword} {typeName}");
            currentType = currentType.ContainingType;
        }
        typeDeclarations.Reverse();
        return typeDeclarations;
    }

    private static bool HasFieldInitialiser(IFieldSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<VariableDeclaratorSyntax>().Any(x => x.Initializer != null);
    }

    public override bool Equals(object obj) => obj is TypeModel model && Equals(model);
    public bool Equals(TypeModel other)
    {
        return Data.Equals(other.Data)
        && Equal(TypeDeclarations, other.TypeDeclarations)
        && Equal(Fields, other.Fields)
        && Equal(BaseCtorParameters, other.BaseCtorParameters)
        && Equal(BaseTypeArguments, other.BaseTypeArguments)
        && Equal(BaseTypeParameters, other.BaseTypeParameters)
        ;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Data.GetHashCode();
            hashCode = (hashCode * 397) ^ ComputeHashCode(TypeDeclarations);
            hashCode = (hashCode * 397) ^ ComputeHashCode(Fields);
            if (BaseCtorParameters != null)
                hashCode = (hashCode * 397) ^ ComputeHashCode(BaseCtorParameters);
            if (BaseTypeArguments != null)
                hashCode = (hashCode * 397) ^ ComputeHashCode(BaseTypeArguments);
            if (BaseTypeParameters != null)
                hashCode = (hashCode * 397) ^ ComputeHashCode(BaseTypeParameters);
            return hashCode;
        }
    }

    private static bool Equal<T>(IReadOnlyList<T>? list1, IReadOnlyList<T>? list2)
    {
        if (list1 is null)
            return list2 is null;
        if (list2 is null)
            return list1 is null;

        if (list1.Count != list2.Count)
            return false;

        for (var i = 0; i < list1.Count; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(list1[i], list2[i]))
                return false;
        }

        return true;
    }
    private static int ComputeHashCode<T>(IReadOnlyList<T> collection)
    {
        var hashCode = typeof(T).GetHashCode();
        for (var i = 0; i < collection.Count; i++)
        {
            hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(collection[i]);
        }
        return hashCode;
    }
}

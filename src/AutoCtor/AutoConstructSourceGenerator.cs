using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AutoCtor;

[Generator(LanguageNames.CSharp)]
public class AutoConstructSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var types = context.SyntaxProvider.CreateSyntaxProvider(IsCorrectAttribute, GetTypeFromAttribute)
            .Where(t => t != null)
            .Collect();

#pragma warning disable CS8622
        context.RegisterSourceOutput(types, GenerateSource);
#pragma warning restore CS8622
    }

    private static bool IsCorrectAttribute(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        if (syntaxNode is not AttributeSyntax attribute)
            return false;

        var name = attribute.Name switch
        {
            SimpleNameSyntax ins => ins.Identifier.Text,
            QualifiedNameSyntax qns => qns.Right.Identifier.Text,
            _ => null
        };

        return IsCorrectAttributeName(name);
    }

    private static bool IsCorrectAttributeName(string? name) =>
        name == "AutoConstruct" || name == "AutoConstructAttribute";

    private static ITypeSymbol? GetTypeFromAttribute(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        // "attribute.Parent" is "AttributeListSyntax"
        // "attribute.Parent.Parent" is a C# fragment the attributes are applied to
        TypeDeclarationSyntax? typeNode = attributeSyntax.Parent?.Parent switch
        {
            ClassDeclarationSyntax classDeclarationSyntax => classDeclarationSyntax,
            RecordDeclarationSyntax recordDeclarationSyntax => recordDeclarationSyntax,
            StructDeclarationSyntax structDeclarationSyntax => structDeclarationSyntax,
            _ => null
        };

        if (typeNode == null)
            return null;

        if (context.SemanticModel.GetDeclaredSymbol(typeNode) is not ITypeSymbol type)
            return null;

        return type;
    }

    private static void GenerateSource(SourceProductionContext context, ImmutableArray<ITypeSymbol> types)
    {
        if (types.IsDefaultOrEmpty)
            return;

        var ctorMaps = new Dictionary<ITypeSymbol, IEnumerable<Parameter>>(SymbolEqualityComparer.Default);

        var baseTypes = types.Where(t => t.BaseType == null || !types.Contains(t.BaseType));
        var extendedTypes = types.Except(baseTypes);

        foreach (var type in baseTypes.Concat(extendedTypes).OfType<INamedTypeSymbol>())
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            IEnumerable<Parameter>? baseParameters = default;

            if (type.BaseType != null)
            {
                if (type.BaseType.IsGenericType)
                {
                    if (ctorMaps.TryGetValue(type.BaseType.ConstructUnboundGenericType(), out var temp))
                    {
                        baseParameters = temp.ToArray();
                        var typedArgs = type.BaseType.TypeArguments;
                        var typedParameters = type.BaseType.TypeParameters;
                        foreach (var bp in baseParameters)
                        {
                            for (var i = 0; i < typedParameters.Length; i++)
                            {
                                if (SymbolEqualityComparer.Default.Equals(typedParameters[i], bp.Type))
                                {
                                    bp.Type = typedArgs[i];
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    ctorMaps.TryGetValue(type.BaseType, out baseParameters);
                }
            }

            (var source, var parameters) = GenerateSource(type, baseParameters);

            if (type.IsGenericType)
                ctorMaps.Add(type.ConstructUnboundGenericType(), parameters);
            else
                ctorMaps.Add(type, parameters);

            var hintSymbolDisplayFormat = new SymbolDisplayFormat(
                globalNamespaceStyle:
                    SymbolDisplayGlobalNamespaceStyle.Omitted,
                typeQualificationStyle:
                    SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions:
                    SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

            var hintName = type.ToDisplayString(hintSymbolDisplayFormat)
                .Replace('<', '[')
                .Replace('>', ']');

            context.AddSource($"{hintName}.g.cs", source);
        }
    }

    private static bool HasFieldInitialiser(IFieldSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<VariableDeclaratorSyntax>().Any(x => x.Initializer != null);
    }

    private static (SourceText, IEnumerable<Parameter>) GenerateSource(ITypeSymbol type, IEnumerable<Parameter>? baseParameters = default)
    {
        var ns = type.ContainingNamespace.IsGlobalNamespace
                ? null
                : type.ContainingNamespace.ToString();

        var fields = type.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.IsReadOnly && !f.IsStatic && f.CanBeReferencedByName && !HasFieldInitialiser(f));

        var parameters = fields.Select(CreateParameter);

        var baseCtorParameters = Enumerable.Empty<Parameter>();
        var baseCtorArgs = Enumerable.Empty<string>();

        if (type.BaseType != null)
        {
            var constructor = type.BaseType.Constructors.OnlyOrDefault(c => !c.IsStatic && c.Parameters.Any());
            if (constructor != null)
            {
                baseCtorParameters = constructor.Parameters.Select(CreateParameter);
                baseCtorArgs = constructor.Parameters.Select(p => CreateFriendlyName(p.Name));
                parameters = baseCtorParameters.Concat(parameters);
            }
            else if (baseParameters != null)
            {
                baseCtorParameters = baseParameters.ToArray();
                baseCtorArgs = baseParameters.Select(p => p.Name).ToArray();
                parameters = baseCtorParameters.Concat(parameters);
            }
        }

        var typeKeyword = type.IsRecord ? "record" : "class";

        var source = new CodeBuilder();
        source.AppendLine($"//------------------------------------------------------------------------------");
        source.AppendLine($"// <auto-generated>");
        source.AppendLine($"//     This code was generated by https://github.com/distantcam/AutoCtor");
        source.AppendLine($"//     Version: {ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}");
        source.AppendLine($"//     SHA: {ThisAssembly.Git.Commit}");
        source.AppendLine($"//");
        source.AppendLine($"//     Changes to this file may cause incorrect behavior and will be lost if");
        source.AppendLine($"//     the code is regenerated.");
        source.AppendLine($"// </auto-generated>");
        source.AppendLine($"//------------------------------------------------------------------------------");
        source.AppendLine($"");

        if (ns is not null)
        {
            source.AppendLine($"namespace {ns}");
            source.StartBlock();
        }

        var typeStack = new Stack<string>();

        var containingType = type.ContainingType;
        while (containingType is not null)
        {
            var contTypeKeyword = containingType.IsRecord ? "record" : "class";
            var typeName = containingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            typeStack.Push(contTypeKeyword + " " + typeName);
            containingType = containingType.ContainingType;
        }

        var nestedCount = typeStack.Count;

        while (typeStack.Count > 0)
        {
            source.AppendLine($"partial {typeStack.Pop()}");
            source.StartBlock();
        }

        source.AppendLine($"partial {typeKeyword} {type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}");
        source.StartBlock();

        if (baseCtorParameters.Any())
        {
            source.AppendLine($"public {type.Name}({ParamString(parameters)}) : base({string.Join(", ", baseCtorArgs)})");
        }
        else
            source.AppendLine($"public {type.Name}({ParamString(parameters)})");
        source.StartBlock();

        foreach (var item in fields)
        {
            source.AppendLine($"this.{item.Name} = {CreateFriendlyName(item.Name)};");
        }

        source.EndBlock();
        source.EndBlock();

        for (var i = 0; i < nestedCount; i++)
        {
            source.EndBlock();
        }

        if (ns is not null)
        {
            source.EndBlock();
        }

        return (source, parameters);
    }

    private static string CreateFriendlyName(string name)
    {
        if (name.Length > 1 && name[0] == '_')
        {
            // Chop off the underscore at the start
            return name.Substring(1);
        }

        return name;
    }

    private static Parameter CreateParameter(IFieldSymbol f) =>
        new Parameter { Type = f.Type, Name = CreateFriendlyName(f.Name) };
    private static Parameter CreateParameter(IParameterSymbol p) =>
        new Parameter { Type = p.Type, Name = CreateFriendlyName(p.Name) };

    private static string ParamString(IEnumerable<Parameter> p) =>
        string.Join(", ", p);

    private class Parameter
    {
        public ITypeSymbol Type;
        public string Name;

        public override string ToString() =>
            $"{Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {Name}";
    }
}

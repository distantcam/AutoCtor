using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using AutoSource;

namespace AutoCtor;

[Generator(LanguageNames.CSharp)]
public class AutoConstructSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var types = context.SyntaxProvider.CreateSyntaxProvider(
            static (s, ct) => SourceTools.IsCorrectAttribute("AutoConstruct", s, ct),
            SourceTools.GetTypeFromAttribute)
            .Where(t => t != null)
            .Collect();

        context.RegisterSourceOutput(types, GenerateSource);
    }

    private static void GenerateSource(SourceProductionContext context, ImmutableArray<ITypeSymbol?> types)
    {
        if (types.IsDefaultOrEmpty)
            return;

        var ctorMaps = new Dictionary<ITypeSymbol, IEnumerable<Parameter>>(SymbolEqualityComparer.Default);
        var orderedTypes = types.OfType<INamedTypeSymbol>().OrderBy(t =>
        {
            var count = 0;
            var b = t.BaseType;
            while (b != null)
            {
                count++;
                b = b.BaseType;
            }
            return count;
        });

        foreach (var type in orderedTypes)
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

            var hintName = type.ToDisplayString(GeneratorUtilities.HintSymbolDisplayFormat)
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

        var source = new CodeBuilder()
            .AppendHeader()
            .AppendLine();

        using (source.StartPartialType(type))
        {
            source.AddCompilerGeneratedAttribute().AddGeneratedCodeAttribute();

            if (baseCtorParameters.Any())
            {
                source.AppendLine($"public {type.Name}({parameters.AsCommaSeparated()}) : base({string.Join(", ", baseCtorArgs)})");
            }
            else
                source.AppendLine($"public {type.Name}({parameters.AsCommaSeparated()})");

            source.StartBlock();
            foreach (var item in fields)
            {
                source.AppendLine($"this.{item.Name} = {CreateFriendlyName(item.Name)};");
            }
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

    private static Parameter CreateParameter(IFieldSymbol f) => new(f.Type, CreateFriendlyName(f.Name));
    private static Parameter CreateParameter(IParameterSymbol p) => new(p.Type, CreateFriendlyName(p.Name));

    private class Parameter
    {
        public Parameter(ITypeSymbol type, string name)
        {
            Type = type;
            Name = name;
        }

        public ITypeSymbol Type { get; set; }
        public string Name { get; set; }

        public override string ToString() =>
            $"{Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {Name}";
    }
}

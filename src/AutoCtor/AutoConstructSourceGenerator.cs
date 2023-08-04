using System.Collections;
using System.Collections.Immutable;
using AutoSource;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
        if (types.IsDefaultOrEmpty) return;

        var ctorMaps = new Dictionary<ITypeSymbol, ParameterList>(SymbolEqualityComparer.Default);
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
                        var baseParameterList = new List<Parameter>();
                        var typedArgs = type.BaseType.TypeArguments;
                        var typedParameters = type.BaseType.TypeParameters;
                        foreach (var bp in temp)
                        {
                            var bpName = bp.Name;
                            var bpType = bp.Type;
                            for (var i = 0; i < typedParameters.Length; i++)
                            {
                                if (SymbolEqualityComparer.Default.Equals(typedParameters[i], bp.Type))
                                {
                                    bpType = typedArgs[i];
                                    break;
                                }
                            }
                            baseParameterList.Add(new Parameter(bpType, bpName));
                        }
                        baseParameters = baseParameterList;
                    }
                }
                else
                {
                    ctorMaps.TryGetValue(type.BaseType, out var temp);
                    baseParameters = temp?.ToList();
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

    private static (SourceText, ParameterList) GenerateSource(ITypeSymbol type, IEnumerable<Parameter>? baseParameters = default)
    {
        var ns = type.ContainingNamespace.IsGlobalNamespace
                ? null
                : type.ContainingNamespace.ToString();

        var fields = type.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.IsReadOnly && !f.IsStatic && f.CanBeReferencedByName && !HasFieldInitialiser(f));

        var parameters = new ParameterList(fields);

        if (type.BaseType != null)
        {
            var constructor = type.BaseType.Constructors.OnlyOrDefault(c => !c.IsStatic && c.Parameters.Any());
            if (constructor != null)
            {
                parameters.AddParameters(constructor.Parameters);
            }
            else if (baseParameters != null)
            {
                parameters.AddBaseParameters(baseParameters);
            }
        }

        parameters.MakeUniqueNames();

        var source = new CodeBuilder()
            .AppendHeader()
            .AppendLine();

        using (source.StartPartialType(type))
        {
            source.AddCompilerGeneratedAttribute().AddGeneratedCodeAttribute();

            if (parameters.HasBaseParameters)
            {
                source.AppendLine($"public {type.Name}({parameters.ToParameterString()}) : base({parameters.ToBaseParameterString()})");
            }
            else
            {
                source.AppendLine($"public {type.Name}({parameters.ToParameterString()})");
            }

            source.StartBlock();
            foreach (var f in fields)
            {
                source.AppendLine($"this.{f.Name} = {parameters.FieldParameterName(f)};");
            }
            source.EndBlock();
        }

        return (source, parameters);
    }

    private static bool HasFieldInitialiser(IFieldSymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<VariableDeclaratorSyntax>().Any(x => x.Initializer != null);
    }
}

public class Parameter
{
    public Parameter(ITypeSymbol type, string name)
    {
        Type = type;
        Name = name;
    }

    public ITypeSymbol Type { get; }
    public string Name { get; set; }

    public override string ToString() =>
        $"{Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {Name}";
}

public class ParameterList : IEnumerable<Parameter>
{
    private readonly Dictionary<IFieldSymbol, Parameter> _fields = new(SymbolEqualityComparer.Default);
    private readonly List<Parameter> _parameters = new();
    private readonly Dictionary<Parameter, string> _uniqueNames = new();

    public bool HasBaseParameters => _parameters.Count > 0;

    public ParameterList(IEnumerable<IFieldSymbol> fields)
    {
        foreach (var f in fields)
        {
            _fields.Add(f, new Parameter(f.Type, CreateFriendlyName(f.Name)));
        }
    }

    public void AddParameters(IEnumerable<IParameterSymbol> parameters)
    {
        foreach (var p in parameters)
        {
            _parameters.Add(new Parameter(p.Type, CreateFriendlyName(p.Name)));
        }
    }

    public void AddBaseParameters(IEnumerable<Parameter> baseParameters)
    {
        foreach (var p in baseParameters)
        {
            _parameters.Add(new Parameter(p.Type, p.Name));
        }
    }

    public void MakeUniqueNames()
    {
        _uniqueNames.Clear();
        var nameHash = new HashSet<string>();
        foreach (var p in this)
        {
            var i = 0;
            var name = p.Name;
            while (nameHash.Contains(name))
            {
                name = p.Name + i++;
            }
            nameHash.Add(name);
            _uniqueNames.Add(p, name);
        }
    }

    public string ToParameterString()
    {
        return this
            .Select(p => $"{p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {_uniqueNames[p]}")
            .AsCommaSeparated();
    }

    public string ToBaseParameterString()
    {
        return _parameters.Select(p => _uniqueNames[p]).AsCommaSeparated();
    }

    public string FieldParameterName(IFieldSymbol f) => _uniqueNames[_fields[f]];
    public IEnumerable<Parameter> BaseParameters() => _parameters;

    public IEnumerator<Parameter> GetEnumerator() => _parameters.Concat(_fields.Values).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static string CreateFriendlyName(string name)
    {
        if (name.Length > 1 && name[0] == '_')
        {
            // Chop off the underscore at the start
            return name.Substring(1);
        }
        return name;
    }
}

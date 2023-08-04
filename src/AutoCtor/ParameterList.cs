using System.Collections;
using Microsoft.CodeAnalysis;

namespace AutoCtor;

public record Parameter(ITypeSymbol Type, string Name);

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

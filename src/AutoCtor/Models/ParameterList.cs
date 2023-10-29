using System.Collections;
using Microsoft.CodeAnalysis;

namespace AutoCtor.Models;

internal record struct Parameter(ITypeSymbol Type, string Name);

internal class ParameterList : IEnumerable<Parameter>
{
    private readonly Dictionary<IFieldSymbol, Parameter> _fields;
    private readonly List<Parameter> _parameters = new();
    private readonly Dictionary<Parameter, string> _uniqueNames = new();
    private IEnumerable<Parameter> _postCtorParameters = Enumerable.Empty<Parameter>();

    public bool HasBaseParameters => _parameters?.Any() == true;

    public ParameterList(IEnumerable<IFieldSymbol> fields)
    {
        _fields = fields.ToDictionary<IFieldSymbol, IFieldSymbol, Parameter>(
            f => f,
            f => new Parameter(f.Type, CreateFriendlyName(f.Name)),
            SymbolEqualityComparer.Default);
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

    public void AddPostCtorParameters(IEnumerable<IParameterSymbol> parameters)
    {
        _postCtorParameters = parameters
            .Select(p => new Parameter(p.Type, CreateFriendlyName(p.Name)))
            .ToArray();
    }

    public void MakeUniqueNames()
    {
        _uniqueNames.Clear();
        var nameHash = new HashSet<string>();
        foreach (var p in this)
        {
            if (_uniqueNames.ContainsKey(p))
                continue;

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
        return _uniqueNames
            .Select(u => $"{u.Key.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {u.Value}")
            .AsCommaSeparated();
    }

    public string ToBaseParameterString()
    {
        return _parameters.Select(p => _uniqueNames[p]).AsCommaSeparated();
    }

    public string ToPostCtorParameterString()
    {
        return _postCtorParameters.Select(p => _uniqueNames[p]).AsCommaSeparated();
    }

    public string FieldParameterName(IFieldSymbol f) => _uniqueNames[_fields[f]];
    public IEnumerable<Parameter> BaseParameters() => _parameters;

    public IEnumerator<Parameter> GetEnumerator() => _parameters
        .Concat(_fields.Values)
        .Concat(_postCtorParameters)
        .GetEnumerator();
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

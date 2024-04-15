using System.Collections;
using Microsoft.CodeAnalysis;

internal record struct Parameter(ITypeSymbol Type, string Name);

internal class ParameterList(IEnumerable<IFieldSymbol> fields, IEnumerable<IPropertySymbol> properties) : IEnumerable<Parameter>
{
    private readonly Dictionary<IFieldSymbol, Parameter> _fields =
        fields.ToDictionary<IFieldSymbol, IFieldSymbol, Parameter>(
            f => f,
            f => new Parameter(f.Type, CreateFriendlyName(f)),
            SymbolEqualityComparer.Default);
    private readonly Dictionary<IPropertySymbol, Parameter> _properties =
        properties.ToDictionary<IPropertySymbol, IPropertySymbol, Parameter>(
            p => p,
            p => new Parameter(p.Type, CreateFriendlyName(p)),
            SymbolEqualityComparer.Default);
    private readonly List<Parameter> _parameters = [];
    private readonly Dictionary<Parameter, string> _uniqueNames = [];
    private IEnumerable<Parameter> _postCtorParameters = [];

    public bool HasBaseParameters => _parameters?.Any() == true;

    public void AddParameters(IEnumerable<IParameterSymbol> parameters)
    {
        foreach (var p in parameters)
        {
            _parameters.Add(new Parameter(p.Type, CreateFriendlyName(p)));
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
            .Select(p => new Parameter(p.Type, CreateFriendlyName(p)))
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

    public string ParameterName(IFieldSymbol f) => _uniqueNames[_fields[f]];
    public string ParameterName(IPropertySymbol p) => _uniqueNames[_properties[p]];

    public IEnumerable<Parameter> BaseParameters() => _parameters;

    public IEnumerator<Parameter> GetEnumerator() => _parameters
        .Concat(_fields.Values)
        .Concat(_properties.Values)
        .Concat(_postCtorParameters)
        .GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static string CreateFriendlyName(IFieldSymbol field)
    {
        if (field.Name.Length > 1 && field.Name[0] == '_')
        {
            // Chop off the underscore at the start
            return field.Name.Substring(1).EscapeKeywordIdentifier();
        }
        return field.Name.EscapeKeywordIdentifier();
    }

    private static string CreateFriendlyName(IPropertySymbol property)
    {
        var name = new string([char.ToLower(property.Name[0]), .. property.Name.Substring(1)]);
        return name.EscapeKeywordIdentifier();
    }

    private static string CreateFriendlyName(IParameterSymbol parameter)
    {
        return parameter.Name.EscapeKeywordIdentifier();
    }
}

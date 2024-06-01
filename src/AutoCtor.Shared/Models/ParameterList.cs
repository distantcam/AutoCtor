using System.Collections;
using Microsoft.CodeAnalysis;

internal class ParameterList(IEnumerable<MemberModel> fields, IEnumerable<MemberModel> properties) : IEnumerable<ParameterModel>
{
    private readonly Dictionary<MemberModel, ParameterModel> _memberToParameterMap =
        fields.Concat(properties)
        .ToDictionary(m => m, m => new ParameterModel(m.FriendlyName, m.TypeName));

    private readonly List<ParameterModel> _parameters = [];
    private readonly Dictionary<ParameterModel, string> _uniqueNames = [];
    private IEnumerable<ParameterModel> _postCtorParameters = [];

    public bool HasBaseParameters => _parameters?.Any() == true;

    public void AddParameters(IEnumerable<ParameterModel> parameters)
    {
        _parameters.AddRange(parameters);
    }

    public void AddPostCtorParameters(IEnumerable<ParameterModel> parameters)
    {
        _postCtorParameters = parameters;
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
            .Select(u => $"{u.Key.TypeName} {u.Value}")
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

    public string ParameterName(MemberModel f) => _uniqueNames[_memberToParameterMap[f]];

    public IEnumerable<ParameterModel> BaseParameters() => _parameters;

    public IEnumerator<ParameterModel> GetEnumerator() => _parameters
        .Concat(_memberToParameterMap.Values)
        .Concat(_postCtorParameters)
        .GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

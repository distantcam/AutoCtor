using System.Collections;
using System.Globalization;

internal class ParameterList(IEnumerable<MemberModel> fields, IEnumerable<MemberModel> properties)
    : IEnumerable<ParameterModel>, IFormattable
{
    private readonly Dictionary<MemberModel, ParameterModel> _memberToParameterMap
        = fields.Concat(properties)
        .ToDictionary(m => m, m => new ParameterModel(m.FriendlyName, m.Type));
    private readonly List<ParameterModel> _parameters = [];
    private readonly Dictionary<ParameterModel, string> _uniqueNames = [];
    private readonly List<ParameterModel> _postCtorParameters = [];

    public bool HasBaseParameters => _parameters?.Any() == true;

    public void AddParameters(IEnumerable<ParameterModel> parameters)
        => _parameters.AddRange(parameters);
    public void AddPostCtorParameters(IEnumerable<ParameterModel> parameters)
        => _postCtorParameters.AddRange(parameters);

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

    public string ParameterName(MemberModel f) => _uniqueNames[_memberToParameterMap[f]];

    public IEnumerator<ParameterModel> GetEnumerator() => _parameters
        .Concat(_memberToParameterMap.Values)
        .Concat(_postCtorParameters)
        .GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => ToString("G", CultureInfo.InvariantCulture);
    public string ToString(string format) => ToString(format, CultureInfo.InvariantCulture);
    public string ToString(string format, IFormatProvider provider)
    {
        if (string.IsNullOrEmpty(format)) format = "G";
        return format.ToUpperInvariant() switch
        {
            "G" => _uniqueNames.Select(u => $"{u.Key.Type} {u.Value}").AsCommaSeparated(),
            "B" => _parameters.Select(p => _uniqueNames[p]).AsCommaSeparated(),
            "P" => _postCtorParameters.Select(p => _uniqueNames[p]).AsCommaSeparated(),
            _ => throw new FormatException($"The {format} format string is not supported"),
        };
    }
}

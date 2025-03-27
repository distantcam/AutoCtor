using System.Collections;
using Microsoft.CodeAnalysis;

internal class ParameterListBuilder(IEnumerable<MemberModel> fields, IEnumerable<MemberModel> properties)
{
    private IEnumerable<ParameterModel> _baseParameters = [];
    private IEnumerable<ParameterModel> _postCtorParameters = [];

    public void AddBaseParameters(IEnumerable<ParameterModel> baseParameters)
        => _baseParameters = baseParameters;

    public void AddPostCtorParameters(IEnumerable<ParameterModel> postCtorParameters)
        => _postCtorParameters = postCtorParameters;

    public ParameterList Build()
    {
        var baseParameters = new List<string>();
        var postCtorParameters = new List<string>();
        var parametersMap = new Dictionary<MemberModel, string>();
        var parameterModels = new List<ParameterModel>();

        var nameHash = new HashSet<string>();
        var uniqueNames = new Dictionary<ParameterModel, string>();

        foreach (var p in _baseParameters)
        {
            var name = GetUniqueName(p, nameHash, uniqueNames);
            parameterModels.Add(p);

            baseParameters.Add(name);
        }
        foreach (var m in fields)
        {
            if (_postCtorParameters.Any(p => p.Type == m.Type
                && (p.RefKind == RefKind.Ref || p.RefKind == RefKind.Out)))
                continue;

            var p = new ParameterModel(RefKind.None, m.FriendlyName, m.Type);
            var name = GetUniqueName(p, nameHash, uniqueNames);
            parameterModels.Add(p);

            parametersMap.Add(m, name);
        }
        foreach (var m in properties)
        {
            var p = new ParameterModel(RefKind.None, m.FriendlyName, m.Type);
            var name = GetUniqueName(p, nameHash, uniqueNames);
            parameterModels.Add(p);

            parametersMap.Add(m, name);
        }
        foreach (var p in _postCtorParameters)
        {
            var matchingField = fields.FirstOrDefault(m => m.Type == p.Type
                && (p.RefKind == RefKind.Ref || p.RefKind == RefKind.Out));
            if (matchingField != default)
            {
                postCtorParameters.Add(p.RefKind != RefKind.None
                    ? $"{p.RefKind.ToParameterPrefix()} {matchingField.IdentifierName}"
                    : matchingField.IdentifierName);
                continue;
            }

            var name = GetUniqueName(p, nameHash, uniqueNames);
            parameterModels.Add(p);

            postCtorParameters.Add(p.RefKind != RefKind.None
                ? $"{p.RefKind.ToParameterPrefix()} {name}" : name);
        }

        var constructorParameters = uniqueNames.Select(u => $"{u.Key.Type} {u.Value}").ToList();

        return new(
            constructorParameters,
            baseParameters,
            postCtorParameters,
            parametersMap,
            parameterModels
        );
    }

    private string GetUniqueName(ParameterModel p, HashSet<string> nameHash, Dictionary<ParameterModel, string> uniqueNames)
    {
        if (uniqueNames.TryGetValue(p, out var name))
            return name;

        var i = 0;
        name = p.Name;
        while (nameHash.Contains(name))
        {
            name = p.Name + i++;
        }
        nameHash.Add(name);
        uniqueNames.Add(p, name);
        return name;
    }
}

internal class ParameterList(
    IEnumerable<string> ctorParameterDeclarations,
    IEnumerable<string> baseParameters,
    IEnumerable<string> postCtorParameters,
    Dictionary<MemberModel, string> parameterMap,
    IEnumerable<ParameterModel> parameterModels
) : IEnumerable<ParameterModel>
{
    public bool HasBaseParameters => baseParameters.Any();
    public IEnumerable<string> CtorParameterDeclarations => ctorParameterDeclarations;
    public IEnumerable<string> BaseParameters => baseParameters;
    public IEnumerable<string> PostCtorParameters => postCtorParameters;

    public string? GetParameter(MemberModel m) =>
        parameterMap.TryGetValue(m, out var result) ? result : null;

    public IEnumerator<ParameterModel> GetEnumerator() => parameterModels.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

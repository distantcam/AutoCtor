using System.Collections;
using Microsoft.CodeAnalysis;

#if ROSLYN_3
using EmitterContext = Microsoft.CodeAnalysis.GeneratorExecutionContext;
#elif ROSLYN_4
using EmitterContext = Microsoft.CodeAnalysis.SourceProductionContext;
#endif

internal sealed class ParameterListBuilder(IEnumerable<MemberModel> fields, IEnumerable<MemberModel> properties)
{
    private IEnumerable<ParameterModel> _baseParameters = [];
    private IEnumerable<ParameterModel> _postCtorParameters = [];

    public void SetBaseParameters(IEnumerable<ParameterModel> baseParameters)
        => _baseParameters = baseParameters;

    public void SetPostCtorParameters(IEnumerable<ParameterModel> postCtorParameters)
        => _postCtorParameters = postCtorParameters;

    public ParameterList Build(EmitterContext context)
    {
        var baseParameters = new List<string>();
        var postCtorParameters = new List<string>();
        var parametersMap = new Dictionary<MemberModel, string>();
        var parameterModels = new List<ParameterModel>();

        var nameHash = new HashSet<string>();
        var uniqueNames = new Dictionary<ParameterModel, string>();

        foreach (var p in _baseParameters)
        {
            if (GetUniqueName(p, nameHash, uniqueNames, out var name))
                continue;

            parameterModels.Add(p);
            baseParameters.Add(name);
        }
        foreach (var m in fields)
        {
            // ref/out from postctor
            if (_postCtorParameters.Any(p => p.IsOutOrRef && m.Type == p.Type))
                continue;

            var p = ParameterModel.Create(m);
            GetUniqueName(p, nameHash, uniqueNames, out var name);
            parameterModels.Add(p);

            parametersMap.Add(m, name);
        }
        foreach (var m in properties)
        {
            // properties cannot be out parameters, so no need to do the same check as fields

            var p = ParameterModel.Create(m);
            GetUniqueName(p, nameHash, uniqueNames, out var name);
            parameterModels.Add(p);

            parametersMap.Add(m, name);
        }

        foreach (var p in _postCtorParameters)
        {
            var matchingField = p.IsOutOrRef
                ? fields.FirstOrDefault(m => m.Type == p.Type)
                : default;
            if (matchingField != default)
            {
                postCtorParameters.Add(p.RefKind != RefKind.None
                    ? $"{p.RefKind.ToParameterPrefix()} {matchingField.IdentifierName}"
                    : matchingField.IdentifierName);
                continue;
            }

            GetUniqueName(p, nameHash, uniqueNames, out var name);
            parameterModels.Add(p);

            postCtorParameters.Add(p.RefKind != RefKind.None
                ? $"{p.RefKind.ToParameterPrefix()} {name}" : name);
        }

        var constructorParameters = uniqueNames.Select(ConstructorParameterCSharp).ToList();

        return new(
            constructorParameters,
            baseParameters,
            postCtorParameters,
            parametersMap,
            parameterModels
        );
    }

    private static string ConstructorParameterCSharp(KeyValuePair<ParameterModel, string> u)
    {
        var defaultValue = u.Key.HasExplicitDefaultValue
            ? $" = {u.Key.ExplicitDefaultValue}"
            : string.Empty;

        if (u.Key.KeyedService != null)
            return $"[global::Microsoft.Extensions.DependencyInjection.FromKeyedServices({u.Key.KeyedService})] {u.Key.Type} {u.Value}{defaultValue}";

        return $"{u.Key.Type} {u.Value}{defaultValue}";
    }

    private static bool GetUniqueName(ParameterModel p, HashSet<string> nameHash, Dictionary<ParameterModel, string> uniqueNames, out string name)
    {
        if (uniqueNames.TryGetValue(p, out name))
            return true;

        var i = 0;
        name = p.Name;
        while (nameHash.Contains(name))
        {
            name = p.Name + i++;
        }
        nameHash.Add(name);
        uniqueNames.Add(p, name);
        return false;
    }
}

internal sealed class ParameterList(
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

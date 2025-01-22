using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Xunit.Abstractions;

public record CodeFileTheoryData : IXunitSerializable
{
    public required string Name { get; set; }
    public required string[] Codes { get; set; }
    public required string VerifiedDirectory { get; set; }

    public (string, string)[] Options { get; set; } = [];
    public bool LangPreview { get; set; }
    public string[] IgnoredCompileDiagnostics { get; set; } = [];

    [SetsRequiredMembers]
    public CodeFileTheoryData(string file, params string[] codes)
    {
        Name = Path.GetFileNameWithoutExtension(file);
        Codes = [File.ReadAllText(file), .. codes];
        VerifiedDirectory = Path.Combine(Path.GetDirectoryName(file) ?? "", "Verified");
    }

    public CodeFileTheoryData() { }

    public void Deserialize(IXunitSerializationInfo info)
    {
        Name = info.GetValue<string>(nameof(Name))
            ?? throw new Exception($"Missing {nameof(Name)} in theory serialization");
        Codes = info.GetValue<string[]>(nameof(Codes))
            ?? throw new Exception($"Missing {nameof(Codes)} in theory serialization");
        VerifiedDirectory = info.GetValue<string>(nameof(VerifiedDirectory))
            ?? throw new Exception($"Missing {nameof(VerifiedDirectory)} in theory serialization");
        Options = info.GetValue<string[]>(nameof(Options))?
            .Select(o => o.Split('|'))
            .Select(o => (o[0], o[1]))
            .ToArray()
            ?? throw new Exception($"Missing {nameof(Options)} in theory serialization");
        LangPreview = info.GetValue<bool>(nameof(LangPreview));
        IgnoredCompileDiagnostics = info.GetValue<string[]>(nameof(IgnoredCompileDiagnostics))
            ?? throw new Exception($"Missing {nameof(IgnoredCompileDiagnostics)} in theory serialization");
    }

    public void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue(nameof(Name), Name);
        info.AddValue(nameof(Codes), Codes);
        info.AddValue(nameof(VerifiedDirectory), VerifiedDirectory);
        info.AddValue(nameof(Options), Options.Select(o => $"{o.Item1}|{o.Item2}").ToArray());
        info.AddValue(nameof(LangPreview), LangPreview);
        info.AddValue(nameof(IgnoredCompileDiagnostics), IgnoredCompileDiagnostics);
    }

    public override string ToString() => Name + ".cs";
}

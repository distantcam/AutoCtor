using System.Diagnostics.CodeAnalysis;

public record CodeFileTheoryData
{
    public required string Name { get; set; }
    public required string[] Codes { get; set; }
    public required string VerifiedDirectory { get; set; }

    public Dictionary<string, string> Options { get; set; } = [];
    public bool LangPreview { get; set; }
    public string[] IgnoredCompileDiagnostics { get; set; } = [];

    [SetsRequiredMembers]
    public CodeFileTheoryData(string file, params string[] codes)
    {
        Name = Path.GetFileNameWithoutExtension(file);
        Codes = [File.ReadAllText(file), .. codes];
        VerifiedDirectory = Path.GetDirectoryName(file) ?? "";
    }

    public override string ToString() => Name + ".cs";
}

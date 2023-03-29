using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace AutoCtor;

internal class CodeBuilder
{
    private readonly StringBuilder _stringBuilder = new();
    private int _indent = 0;

    public void StartBlock()
    {
        AppendLine("{");
        IncreaseIndent();
    }
    public void EndBlock()
    {
        DecreaseIndent();
        AppendLine("}");
    }
    public void IncreaseIndent() => _indent++;
    public void DecreaseIndent() => _indent--;
    public void AppendLine(string line) => _stringBuilder.AppendLine(new string('\t', _indent) + line);
    public void AppendLines(IEnumerable<string> lines)
    {
        foreach (var line in lines)
            AppendLine(line.TrimEnd('\r'));
    }
    public static implicit operator SourceText(CodeBuilder codeBuilder) => SourceText.From(codeBuilder._stringBuilder.ToString(), Encoding.UTF8);
}

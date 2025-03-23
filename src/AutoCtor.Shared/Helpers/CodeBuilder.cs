using System.Text;
using Microsoft.CodeAnalysis.Text;

internal partial class CodeBuilder
{
    private readonly StringBuilder _stringBuilder = new();
    private int _indent = 0;

    public char IndentChar { get; set; } = '\t';
    public string Indent => new(IndentChar, _indent);

    public CodeBuilder IncreaseIndent() { _indent++; return this; }
    public CodeBuilder DecreaseIndent() { if (_indent > 0) _indent--; return this; }

    public CodeBuilder Append(string text) { _stringBuilder.Append(text); return this; }
    public CodeBuilder Append(bool enabled, string text) { if (enabled) _stringBuilder.Append(text); return this; }
    public CodeBuilder AppendLine() { _stringBuilder.AppendLine(); return this; }
    public CodeBuilder AppendLine(string line) { _stringBuilder.AppendLine(Indent + line); return this; }
    public CodeBuilder AppendLine(bool enabled, string line) { if (enabled) _stringBuilder.AppendLine(Indent + line); return this; }
    public CodeBuilder AppendLineRaw(string line) { _stringBuilder.AppendLine(line); return this; }
    public CodeBuilder AppendLineRaw(bool enabled, string line) { if (enabled) _stringBuilder.AppendLine(line); return this; }
    public CodeBuilder AppendIndent() { _stringBuilder.Append(Indent); return this; }

    public static implicit operator SourceText(CodeBuilder codeBuilder)
        => SourceText.From(codeBuilder._stringBuilder.ToString(), Encoding.UTF8);
}

using System.Text;
using Microsoft.CodeAnalysis.Text;

internal partial class CodeBuilder
{
    private readonly StringBuilder _stringBuilder = new();
    private int _indent;

    public char IndentChar { get; set; } = '\t';
    public string Indent => new(IndentChar, _indent);

    public CodeBuilder IncreaseIndent() { _indent++; return this; }
    public CodeBuilder DecreaseIndent() { if (_indent > 0) _indent--; return this; }

    public CodeBuilder Append(string value) { _stringBuilder.Append(value); return this; }
    public CodeBuilder Append(bool enabled, string value) { if (enabled) _stringBuilder.Append(value); return this; }
    public CodeBuilder AppendLine() { _stringBuilder.AppendLine(); return this; }
    public CodeBuilder AppendLine(string value) { _stringBuilder.AppendLine(Indent + value); return this; }
    public CodeBuilder AppendLine(bool enabled, string value) { if (enabled) _stringBuilder.AppendLine(Indent + value); return this; }
    public CodeBuilder AppendLineRaw(string value) { _stringBuilder.AppendLine(value); return this; }
    public CodeBuilder AppendLineRaw(bool enabled, string value) { if (enabled) _stringBuilder.AppendLine(value); return this; }
    public CodeBuilder AppendIndent() { _stringBuilder.Append(Indent); return this; }

    public static implicit operator SourceText(CodeBuilder codeBuilder)
        => SourceText.From(codeBuilder._stringBuilder.ToString(), Encoding.UTF8);
}

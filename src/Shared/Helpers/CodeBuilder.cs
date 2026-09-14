using System.Text;
using Microsoft.CodeAnalysis.Text;

internal partial class CodeBuilder
{
    private readonly StringBuilder _stringBuilder = new();
    private int _indent;

    //public char IndentChar { get; set; } = '\t';
    // Every emitted line asks for this, so the common tab indents are shared, not rebuilt.
    private static readonly string[] s_tabIndents = [.. Enumerable.Range(0, 16).Select(static i => new string('\t', i))];
    public string Indent => _indent < s_tabIndents.Length
        ? s_tabIndents[_indent]
        : new('\t', _indent);

    public CodeBuilder IncreaseIndent() { _indent++; return this; }
    public CodeBuilder DecreaseIndent() { if (_indent > 0) _indent--; return this; }

    public CodeBuilder Append(string value) { _stringBuilder.Append(value); return this; }
    public CodeBuilder Append(bool enabled, string value) => enabled ? Append(value) : this;

    public CodeBuilder AppendLine() { _stringBuilder.AppendLine(); return this; }
    public CodeBuilder AppendLine(string value) => AppendLineRaw(Indent + value);
    public CodeBuilder AppendLine(bool enabled, string value) => enabled ? AppendLine(value) : this;

    public CodeBuilder AppendLineRaw(string value) { _stringBuilder.AppendLine(value); return this; }
    public CodeBuilder AppendLineRaw(bool enabled, string value) => enabled ? AppendLineRaw(value) : this;

    public CodeBuilder AppendIndent() => Append(Indent);

    public static implicit operator SourceText(CodeBuilder codeBuilder)
        => SourceText.From(codeBuilder._stringBuilder.ToString(), Encoding.UTF8);
}

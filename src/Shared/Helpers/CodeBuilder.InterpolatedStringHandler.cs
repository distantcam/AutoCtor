using System.Runtime.CompilerServices;

#pragma warning disable CS9113 // Parameter is unread.
#pragma warning disable IDE0060 // Remove unused parameter

internal partial class CodeBuilder
{
    public CodeBuilder Append(
        [InterpolatedStringHandlerArgument("")]
        ref CodeBuilderInterpolatedStringHandler builder) => this;

    public CodeBuilder Append(bool enabled,
        [InterpolatedStringHandlerArgument("", nameof(enabled))]
        ref CodeBuilderInterpolatedStringHandler builder) => this;

    public CodeBuilder AppendLineRaw(
        [InterpolatedStringHandlerArgument("")]
        ref CodeBuilderInterpolatedStringHandler builder) => AppendLine();

    public CodeBuilder AppendLineRaw(bool enabled,
        [InterpolatedStringHandlerArgument("", nameof(enabled))]
        ref CodeBuilderInterpolatedStringHandler builder) => enabled ? AppendLine() : this;

    public CodeBuilder AppendLine(
        [InterpolatedStringHandlerArgument("")]
        IndentedCodeBuilderInterpolatedStringHandler builder) => AppendLine();

    public CodeBuilder AppendLine(bool enabled,
        [InterpolatedStringHandlerArgument("", nameof(enabled))]
        IndentedCodeBuilderInterpolatedStringHandler builder) => enabled ? AppendLine() : this;

    private void AppendFormatted(IEnumerable<string> items, string? format)
    {
        if (format == "comma")
            AppendCommaSeparated(items);

        if (format == "commaindent")
            AppendCommaIndented(items);
    }

    private void AppendCommaSeparated(IEnumerable<string> items)
    {
        var comma = false;
        foreach (var item in items)
        {
            if (comma)
                Append(", ");
            comma = true;
            Append(item);
        }
    }

    private void AppendCommaIndented(IEnumerable<string> items)
    {
        var length = items.Sum(s => s.Length);
        if (length < 60)
        {
            AppendCommaSeparated(items);
            return;
        }

        AppendLine();
        IncreaseIndent();
        var comma = false;
        foreach (var item in items)
        {
            if (comma)
                Append(",").AppendLine();
            comma = true;
            AppendIndent().Append(item);
        }
        AppendLine();
        DecreaseIndent();
        AppendIndent();
    }

    [InterpolatedStringHandler]
    internal readonly struct CodeBuilderInterpolatedStringHandler(
        int literalLength, int formattedCount, CodeBuilder codeBuilder, bool enabled = true)
    {
        public readonly bool AppendLiteral(string s)
        { if (enabled) codeBuilder.Append(s); return enabled; }
        public readonly bool AppendFormatted(string s)
        { if (enabled) codeBuilder.Append(s); return enabled; }
        public readonly bool AppendFormatted(IEnumerable<string> items, string? format)
        { if (enabled) codeBuilder.AppendFormatted(items, format); return enabled; }
    }

    [InterpolatedStringHandler]
    internal class IndentedCodeBuilderInterpolatedStringHandler(
        int literalLength, int formattedCount, CodeBuilder codeBuilder, bool enabled = true)
    {
        private bool _hasIndented;

        private CodeBuilder EnsureIndent()
        {
            if (!_hasIndented)
            {
                codeBuilder.AppendIndent();
                _hasIndented = true;
            }
            return codeBuilder;
        }

        public bool AppendLiteral(string s)
        { if (enabled) EnsureIndent().Append(s); return enabled; }
        public bool AppendFormatted(string s)
        { if (enabled) EnsureIndent().Append(s); return enabled; }
        public bool AppendFormatted(IEnumerable<string> items, string? format)
        { if (enabled) EnsureIndent().AppendFormatted(items, format); return enabled; }
    }
}

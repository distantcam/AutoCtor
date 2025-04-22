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
        ref ConditionalCodeBuilderInterpolatedStringHandler builder) => this;

    public CodeBuilder AppendLineRaw(
        [InterpolatedStringHandlerArgument("")]
        ref CodeBuilderInterpolatedStringHandler builder) => AppendLine();

    public CodeBuilder AppendLineRaw(bool enabled,
        [InterpolatedStringHandlerArgument("", nameof(enabled))]
        ref ConditionalCodeBuilderInterpolatedStringHandler builder) => enabled ? AppendLine() : this;

    public CodeBuilder AppendLine(
        [InterpolatedStringHandlerArgument("")]
        IndentedCodeBuilderInterpolatedStringHandler builder) => AppendLine();

    public CodeBuilder AppendLine(bool enabled,
        [InterpolatedStringHandlerArgument("", nameof(enabled))]
        ConditionalIndentedCodeBuilderInterpolatedStringHandler builder) => enabled ? AppendLine() : this;

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
        int literalLength, int formattedCount, CodeBuilder codeBuilder)
    {
        public readonly void AppendLiteral(string s) => codeBuilder.Append(s);
        public readonly void AppendFormatted(string s) => codeBuilder.Append(s);
        public readonly void AppendFormatted(IEnumerable<string> items, string? format)
            => codeBuilder.AppendFormatted(items, format);
    }

    [InterpolatedStringHandler]
    internal readonly struct ConditionalCodeBuilderInterpolatedStringHandler(
        int literalLength, int formattedCount, CodeBuilder codeBuilder, bool enabled)
    {
        public readonly bool AppendLiteral(string s) { if (enabled) codeBuilder.Append(s); return enabled; }
        public readonly bool AppendFormatted(string s) { if (enabled) codeBuilder.Append(s); return enabled; }
        public readonly bool AppendFormatted(IEnumerable<string> items, string? format)
        { if (enabled) codeBuilder.AppendFormatted(items, format); return enabled; }
    }

    [InterpolatedStringHandler]
    internal class IndentedCodeBuilderInterpolatedStringHandler(
        int literalLength, int formattedCount, CodeBuilder codeBuilder)
    {
        private bool _hasIndented;

        private void EnsureIndent()
        {
            if (_hasIndented) return;
            codeBuilder.AppendIndent();
            _hasIndented = true;
        }

        public void AppendLiteral(string s) { EnsureIndent(); codeBuilder.Append(s); }
        public void AppendFormatted(string s) { EnsureIndent(); codeBuilder.Append(s); }
        public void AppendFormatted(IEnumerable<string> items, string? format)
        { EnsureIndent(); codeBuilder.AppendFormatted(items, format); }
    }

    [InterpolatedStringHandler]
    internal class ConditionalIndentedCodeBuilderInterpolatedStringHandler(
        int literalLength, int formattedCount, CodeBuilder codeBuilder, bool enabled)
        : IndentedCodeBuilderInterpolatedStringHandler(literalLength, formattedCount, codeBuilder)
    {
        public new bool AppendLiteral(string s) { if (enabled) base.AppendLiteral(s); return enabled; }
        public new bool AppendFormatted(string s) { if (enabled) base.AppendFormatted(s); return enabled; }
        public new bool AppendFormatted(IEnumerable<string> items, string? format)
        { if (enabled) base.AppendFormatted(items, format); return enabled; }
    }
}

using System.Runtime.CompilerServices;

#pragma warning disable CS9113 // Parameter is unread.
#pragma warning disable IDE0060 // Remove unused parameter

internal partial class CodeBuilder
{
    public CodeBuilder Append(
        [InterpolatedStringHandlerArgument("")]
        ref CodeBuilderInterpolatedStringHandler builder) => this;

    public CodeBuilder AppendLineRaw(
        [InterpolatedStringHandlerArgument("")]
        ref CodeBuilderInterpolatedStringHandler builder) => AppendLine();

    public CodeBuilder AppendLine(
        [InterpolatedStringHandlerArgument("")]
        IndentedCodeBuilderInterpolatedStringHandler builder) => AppendLine();

    private void AppendFormatted(IEnumerable<string> items, string? format)
    {
        if (format == "comma")
            AppendCommaSeparated(items as IReadOnlyList<string> ?? items.ToList());

        else if (format == "commaindent")
            AppendCommaIndented(items as IReadOnlyList<string> ?? items.ToList());
    }

    private void AppendCommaSeparated(IReadOnlyList<string> items)
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

    private void AppendCommaIndented(IReadOnlyList<string> items)
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
        public readonly void AppendFormatted(bool s) => codeBuilder.Append(s);
        public readonly void AppendFormatted(string s) => codeBuilder.Append(s);
        public readonly void AppendFormatted(IEnumerable<string> items, string? format)
            => codeBuilder.AppendFormatted(items, format);
    }

    [InterpolatedStringHandler]
    internal sealed class IndentedCodeBuilderInterpolatedStringHandler(
        int literalLength, int formattedCount, CodeBuilder codeBuilder)
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

        public void AppendLiteral(string s) => EnsureIndent().Append(s);
        public void AppendFormatted(bool s) => EnsureIndent().Append(s);
        public void AppendFormatted(string s) => EnsureIndent().Append(s);
        public void AppendFormatted(IEnumerable<string> items, string? format)
            => EnsureIndent().AppendFormatted(items, format);
    }
}

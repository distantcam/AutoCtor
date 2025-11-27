internal partial class CodeBuilder
{
    public IDisposable StartBlock() => StartIndent("{", "}");
    public IDisposable StartBlock(string line)
    {
        AppendLine(line);
        return StartIndent("{", "}");
    }

    public IDisposable StartIndent(string? startLine = null, string? endLine = null)
    {
        if (!string.IsNullOrEmpty(startLine))
            AppendLine(startLine!);
        IncreaseIndent();
        return new DetentDisposable(this, endLine);
    }

    private readonly struct DetentDisposable(CodeBuilder codeBuilder, string? endLine) : IDisposable
    {
        public void Dispose()
        {
            codeBuilder.DecreaseIndent();
            if (!string.IsNullOrEmpty(endLine))
                codeBuilder.AppendLine(endLine!);
        }
    }
}

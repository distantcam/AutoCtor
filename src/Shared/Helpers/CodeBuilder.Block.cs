internal partial class CodeBuilder
{
    public IDisposable StartBlock()
    {
        AppendLine("{").IncreaseIndent();
        return new SingleBlockDisposable(this);
    }

    public IDisposable StartBlock(string line)
    {
        AppendLine(line);
        return StartBlock();
    }

    public IDisposable StartType(string typeDeclaration, IEnumerable<string>? baseList = null)
    {
        if (baseList is { })
            AppendLine($"{typeDeclaration} : {baseList:commaindent}");
        else
            AppendLine(typeDeclaration);
        AppendLine("{");
        IncreaseIndent();
        return new SingleBlockDisposable(this);
    }

    private readonly struct SingleBlockDisposable(CodeBuilder codeBuilder) : IDisposable
    {
        public void Dispose() => codeBuilder.DecreaseIndent().AppendLine("}");
    }
}

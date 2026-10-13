internal interface IPartialTypeModel
{
    string? Namespace { get; }
    EquatableList<string> TypeDeclarations { get; }
}

internal partial class CodeBuilder
{
    public IDisposable StartPartialType(IPartialTypeModel typeModel, IEnumerable<string>? baseList = null)
    {
        var hasNamespace = !string.IsNullOrEmpty(typeModel.Namespace);
        if (hasNamespace)
        {
            AppendLine($"namespace {typeModel.Namespace!}");
            AppendLine("{");
            IncreaseIndent();
        }

        for (var i = 0; i < typeModel.TypeDeclarations.Count; i++)
        {
            if (i == typeModel.TypeDeclarations.Count - 1 && baseList is { })
                AppendLine($"{typeModel.TypeDeclarations[i]} : {baseList:commaindent}");
            else
                AppendLine(typeModel.TypeDeclarations[i]);
            AppendLine("{");
            IncreaseIndent();
        }

        return new CloseBlockDisposable(this, typeModel.TypeDeclarations.Count + (hasNamespace ? 1 : 0));
    }

    private readonly struct CloseBlockDisposable(CodeBuilder codeBuilder, int count) : IDisposable
    {
        public void Dispose()
        {
            for (var i = 0; i < count; i++)
            {
                codeBuilder.DecreaseIndent();
                codeBuilder.AppendLine("}");
            }
        }
    }
}

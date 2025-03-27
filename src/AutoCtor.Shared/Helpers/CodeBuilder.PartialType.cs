internal interface IPartialTypeModel
{
    string? Namespace { get; }
    IReadOnlyList<string> TypeDeclarations { get; }
}

internal partial class CodeBuilder
{
    public IDisposable StartPartialType(IPartialTypeModel typeModel)
    {
        if (!string.IsNullOrEmpty(typeModel.Namespace))
        {
            AppendLine($"namespace {typeModel.Namespace!}");
            AppendLine("{");
            IncreaseIndent();
        }

        for (var i = 0; i < typeModel.TypeDeclarations.Count; i++)
        {
            AppendLine(typeModel.TypeDeclarations[i]);
            AppendLine("{");
            IncreaseIndent();
        }

        return new CloseBlockDisposable(this, typeModel.TypeDeclarations.Count + (typeModel.Namespace != null ? 1 : 0));
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

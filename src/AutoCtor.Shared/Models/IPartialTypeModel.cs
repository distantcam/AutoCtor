internal interface IPartialTypeModel
{
    string? Namespace { get; }
    IReadOnlyList<string> TypeDeclarations { get; }
}

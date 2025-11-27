using AutoCtor;

internal abstract record RequestBase(string Id)
{
}

[AutoConstruct]
internal partial record OrderRequest : RequestBase
{
}

using AutoCtor;

[AutoConstruct]
public partial class FriendlyParameterNamesTest
{
    private readonly int _;
    private readonly int _2;
    private readonly int _underscorePrefix;
    private readonly int __doubleUnderscorePrefix;
    private readonly int camelCase;
    private readonly int PascalCase;
}


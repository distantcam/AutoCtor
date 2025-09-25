using AutoCtor;

[AutoConstruct]
public partial class UniqueNameTest2 : UniqueNameTestBase
{
    private readonly int _value;
    private int value { get; }

    [AutoPostConstruct]
    private void Initialise(int value) { }
}

[AutoConstruct]
public partial class UniqueNameTestBase
{
    private readonly int _value;
    private int value { get; }

    [AutoPostConstruct]
    private void Initialise(int value) { }
}

using AutoCtor;

[AutoConstruct]
public partial record RecordTest
{
    private readonly int _item;
}

[AutoConstruct]
public partial record struct RecordStructTest
{
    private readonly int _item;
}

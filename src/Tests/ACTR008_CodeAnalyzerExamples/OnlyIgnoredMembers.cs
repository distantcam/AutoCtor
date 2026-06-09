using AutoCtor;

public class OnlyIgnoredMembers
{
    [AutoConstructIgnore]
    private readonly string _value;

    [AutoConstructIgnore]
    private readonly int _count;
}

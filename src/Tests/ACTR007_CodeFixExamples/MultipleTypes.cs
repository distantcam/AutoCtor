public class TargetType
{
    private readonly string _value;

    public TargetType(string value)
    {
        _value = value;
    }
}

public class OtherClass { }

public record OtherRecord { }

public struct OtheStruct { }

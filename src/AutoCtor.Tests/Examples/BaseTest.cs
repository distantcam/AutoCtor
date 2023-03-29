using AutoCtor;

public abstract class SimpleBase
{
    protected SimpleBase(string text)
    {
    }
}

[AutoConstruct]
public partial class BaseTest : SimpleBase
{
    private readonly int _value;
}

[AutoConstruct]
public abstract class ComplexBase
{
    private readonly string _baseValue;
}

[AutoConstruct]
public abstract class ComplexTest : ComplexBase
{
    private readonly string _value;
}

[AutoConstruct]
public partial class BaseBaseClass
{
    private readonly int _number;
}

[AutoConstruct]
public partial class BaseClass : BaseBaseClass
{
    private readonly string _text;
}

[AutoConstruct]
public partial class TheClass : BaseClass
{
    private readonly bool _flag;
}

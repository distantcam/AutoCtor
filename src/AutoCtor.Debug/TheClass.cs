namespace AutoCtor.Debug;

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

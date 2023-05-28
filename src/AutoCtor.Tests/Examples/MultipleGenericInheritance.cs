using AutoCtor;

[AutoConstruct]
public partial class Generic<TA, TB>
{
    protected readonly TA a;
    protected readonly TB b;
}

[AutoConstruct]
public partial class Example1 : Generic<IA, IB>
{
    private readonly IC c;
}

[AutoConstruct]
public partial class Example2 : Generic<ID, IE>
{
    private readonly IF f;
}

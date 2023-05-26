using AutoCtor;

[AutoConstruct]
public abstract partial class InheritanceBaseA
{
    protected readonly IExampleA _exampleA;
}

[AutoConstruct]
public abstract partial class InheritanceBaseB : InheritanceBaseA
{
    protected readonly IExampleB _exampleB;
}

[AutoConstruct]
public abstract partial class InheritanceBaseC : InheritanceBaseB
{
    protected readonly IExampleC _exampleC;
}

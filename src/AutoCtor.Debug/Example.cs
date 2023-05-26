using AutoCtor;

[AutoConstruct]
public partial class GenericBase<T>
{
    protected readonly T _t;
}

[AutoConstruct]
public partial class ConcreteClass : GenericBase<IExampleA>
{
}

[AutoConstruct]
public partial class ConcreteClassWithOtherClass : ConcreteClass
{
    protected readonly IExampleB _exampleB;
}

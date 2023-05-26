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
public partial class ConcreteClassWithGenericArg<T2> : GenericBase<T2>
{
}

[AutoConstruct]
public partial class ConcreteClassWithAnotherField : ConcreteClass
{
    protected readonly IExampleB exampleB;
}

[AutoConstruct]
public partial class GenericBase2<T1, T2>
{
    protected readonly T1 _t1;
    protected readonly T2 _t2;
}

[AutoConstruct]
public partial class ConcreteClass1<T> : GenericBase2<IExampleA, T>
{
}

[AutoConstruct]
public partial class ConcreteClass2 : ConcreteClass1<IExampleB>
{
}

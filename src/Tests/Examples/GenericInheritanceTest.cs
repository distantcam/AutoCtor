public interface IServiceA { }
public interface IServiceB { }

[AutoCtor.AutoConstruct]
public partial class GenericBase<T>
{
    protected readonly T _t;
}

[AutoCtor.AutoConstruct]
public partial class ConcreteClass : GenericBase<IServiceA>
{
}

[AutoCtor.AutoConstruct]
public partial class ConcreteClassWithGenericArg<T2> : GenericBase<T2>
{
}

[AutoCtor.AutoConstruct]
public partial class ConcreteClassWithAnotherField : ConcreteClass
{
    protected readonly IServiceB _serviceb;
}

[AutoCtor.AutoConstruct]
public partial class GenericBase2<T1, T2>
{
    protected readonly T1 _t1;
    protected readonly T2 _t2;
}

[AutoCtor.AutoConstruct]
public partial class ConcreteClass1<T> : GenericBase2<IServiceA, T>
{
}

[AutoCtor.AutoConstruct]
public partial class ConcreteClass2 : ConcreteClass1<IServiceB>
{
}

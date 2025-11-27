public interface IServiceA { }
public interface IServiceB { }
public interface IServiceC { }
public interface IServiceD { }
public interface IServiceE { }

[AutoCtor.AutoConstruct]
public partial class A : B
{
    private readonly IServiceA _serviceA;
}

[AutoCtor.AutoConstruct]
public partial class B : C
{
    private readonly IServiceB _serviceB;
}

[AutoCtor.AutoConstruct]
public partial class C : D
{
    private readonly IServiceC _serviceC;
}

[AutoCtor.AutoConstruct]
public partial class D : E
{
    private readonly IServiceD _serviceD;
}

[AutoCtor.AutoConstruct]
public partial class E : F
{
    private readonly IServiceE _serviceE;
}

public class F
{
}

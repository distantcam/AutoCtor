using AutoCtor;

public interface IA { }
public interface IB { }
public interface IC { }
public interface ID { }
public interface IE { }

[AutoConstruct]
public partial class A : B
{
    private readonly IA a;
}

[AutoConstruct]
public partial class B : C
{
    private readonly IB b;
}

[AutoConstruct]
public partial class C : D
{
    private readonly IC c;
}

[AutoConstruct]
public partial class D : E
{
    private readonly ID d;
}

[AutoConstruct]
public partial class E : F
{
    private readonly IE e;
}

public class F
{
}

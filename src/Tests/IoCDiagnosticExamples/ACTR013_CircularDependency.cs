using AutoCtor;

[ServiceProvider]
[Singleton<ICycleA, CycleA>]
[Singleton<ICycleB, CycleB>]
public partial class CircularDependencyProvider;

public interface ICycleA;
public interface ICycleB;

public class CycleA : ICycleA
{
    public CycleA(ICycleB b) { }
}

public class CycleB : ICycleB
{
    public CycleB(ICycleA a) { }
}

using AutoCtor;

// Registered in reverse dependency order on purpose, to prove the emitter sorts.
[ServiceProvider]
[Singleton<IChainA, ChainA>]
[Singleton<IChainB, ChainB>]
[Singleton<IChainC, ChainC>]
public partial class DependencyChainProvider;

public interface IChainA;
public interface IChainB;
public interface IChainC;

public class ChainC : IChainC;

public class ChainB : IChainB
{
    public ChainB(IChainC c) { }
}

public class ChainA : IChainA
{
    public ChainA(IChainB b, IChainC c) { }
}

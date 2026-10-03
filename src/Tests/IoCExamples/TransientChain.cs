using AutoCtor;

// A transient depending on another transient nests the factory calls, so every level of
// the chain is rebuilt on each resolve.
[ServiceProvider]
[Transient<IChainOuter, ChainOuter>]
[Transient<IChainMiddle, ChainMiddle>]
[Transient<IChainInner, ChainInner>]
public sealed partial class TransientChainProvider;

public interface IChainOuter;
public interface IChainMiddle;
public interface IChainInner;

public class ChainInner : IChainInner;

public class ChainMiddle : IChainMiddle
{
    public ChainMiddle(IChainInner inner) { }
}

public class ChainOuter : IChainOuter
{
    public ChainOuter(IChainMiddle middle, IChainInner inner) { }
}

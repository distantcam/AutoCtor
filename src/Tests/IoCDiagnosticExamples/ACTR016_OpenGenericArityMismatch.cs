using AutoCtor;

// The rule cannot work: closing IPair<,> yields two type arguments, and PairImpl<> only
// takes one.
[ServiceProvider]
[Singleton(typeof(IPair<,>), typeof(PairImpl<>))]
public partial class PairProvider;

public interface IPair<TFirst, TSecond>;
public class PairImpl<T> : IPair<T, T>;

using AutoCtor;

// Nothing is eagerly constructed, so no constructor should be emitted at all -- the
// implicit parameterless one is left alone.
[ServiceProvider]
[Transient<ITransientOnlyOne, TransientOnlyOne>]
[Transient<ITransientOnlyTwo, TransientOnlyTwo>]
public partial class TransientOnlyProvider;

public interface ITransientOnlyOne;
public interface ITransientOnlyTwo;
public class TransientOnlyOne : ITransientOnlyOne;
public class TransientOnlyTwo : ITransientOnlyTwo;

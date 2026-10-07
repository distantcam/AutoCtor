using AutoCtor;

// No singletons at all, so the provider itself has nothing to construct and everything
// lives on the scope.
[ServiceProvider]
[Scoped<IScopedOnlyOne, ScopedOnlyOne>]
[Scoped<IScopedOnlyTwo, ScopedOnlyTwo>]
public sealed partial class ScopedOnlyProvider;

public interface IScopedOnlyOne;
public interface IScopedOnlyTwo;
public class ScopedOnlyOne : IScopedOnlyOne;

public class ScopedOnlyTwo : IScopedOnlyTwo
{
    public ScopedOnlyTwo(IScopedOnlyOne one) { }
}

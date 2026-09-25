using AutoCtor;

// Each element keeps its own lifetime, so the array is rebuilt per resolve: the singleton
// element is read from its field, the transient one is constructed again.
[ServiceProvider]
[Singleton<IStep, CachedStep>]
[Transient<IStep, FreshStep>]
public partial class MixedLifetimeProvider;

public interface IStep;
public class CachedStep : IStep;
public class FreshStep : IStep;

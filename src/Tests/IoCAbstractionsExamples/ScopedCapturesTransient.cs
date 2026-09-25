using AutoCtor;

// A scoped service holds its transient dependency for as long as the scope lives. That is
// what constructor injection of a transient means, and the scope releases it, so this is
// deliberately not reported -- unlike the same capture by a singleton, which holds the
// instance for the life of the process and is ACTR018.
[ServiceProvider]
[Scoped<IScopedHolder, ScopedHolder>]
[Transient<IScopedWorker, ScopedWorker>]
public partial class ScopedCaptureTransientProvider;

public interface IScopedWorker;
public class ScopedWorker : IScopedWorker;

public interface IScopedHolder;

public class ScopedHolder : IScopedHolder
{
    public ScopedHolder(IScopedWorker worker) { }
}

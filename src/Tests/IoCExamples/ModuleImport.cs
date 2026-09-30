using AutoCtor;

// Imports expand where they are written, so the provider's own IClock, written after them,
// wins single resolution over the module's.
[ServiceProvider]
[Import<ClockModule>]
[Import(typeof(GreeterModule))]
[Singleton<IClock, TestClock>]
public sealed partial class ModuleImportProvider;

[Singleton<IClock, SystemClock>]
public class ClockModule;

[Transient<IGreeter, Greeter>]
public class GreeterModule;

public interface IClock;
public class SystemClock : IClock;
public class TestClock : IClock;

public interface IGreeter;
public class Greeter(IClock clock) : IGreeter
{
    public IClock Clock => clock;
}

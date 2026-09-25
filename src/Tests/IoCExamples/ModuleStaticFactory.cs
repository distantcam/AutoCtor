using AutoCtor;

// A module is never instantiated, so its factory is static and called through the module.
// Its parameters still resolve against the provider.
[ServiceProvider]
[Import<GreetingModule>]
[Singleton<IGreetingClock, GreetingClock>]
public partial class ModuleFactoryProvider;

[Transient<IGreeting>(Factory = nameof(Create))]
public class GreetingModule
{
    internal static IGreeting Create(IGreetingClock clock) => new Greeting(clock);
}

public interface IGreetingClock;
public class GreetingClock : IGreetingClock;

public interface IGreeting;
public class Greeting(IGreetingClock clock) : IGreeting
{
    public IGreetingClock Clock => clock;
}

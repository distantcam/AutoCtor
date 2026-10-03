using AutoCtor;

// A static factory has no receiver to choose, so it is called through the provider type.
[ServiceProvider]
[Transient<IGreeting>(Factory = nameof(Create))]
public sealed partial class GreetingProvider
{
    private static IGreeting Create() => new Greeting("hello");
}

public interface IGreeting;
public class Greeting(string text) : IGreeting
{
    public string Text => text;
}

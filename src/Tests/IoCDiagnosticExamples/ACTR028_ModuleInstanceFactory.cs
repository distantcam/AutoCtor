using AutoCtor;

// Build is an instance member, and nothing ever creates an InstanceFactoryModule to call it on.
[ServiceProvider]
[Import<InstanceFactoryModule>]
public partial class InstanceFactoryProvider;

[Singleton<IThing>(Factory = nameof(Build))]
public class InstanceFactoryModule
{
    public IThing Build() => new Thing();
}

public interface IThing;
public class Thing : IThing;

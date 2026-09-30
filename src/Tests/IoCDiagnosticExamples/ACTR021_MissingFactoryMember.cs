using AutoCtor;

// Nothing on the provider is called BuildThing. A name that does exist would be written
// with nameof; a literal is the only way to name one that does not.
[ServiceProvider]
[Singleton(typeof(IThing), typeof(Thing), Factory = "BuildThing")]
public sealed partial class MissingFactoryProvider;

public interface IThing;
public class Thing : IThing;

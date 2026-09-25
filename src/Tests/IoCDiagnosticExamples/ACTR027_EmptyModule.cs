using AutoCtor;

// EmptyModule carries no registrations -- which is also how a module from an assembly built
// without AUTOCTOR_USAGES looks.
[ServiceProvider]
[Import<EmptyModule>]
[Singleton<IThing, Thing>]
public partial class EmptyModuleProvider;

public class EmptyModule;

public interface IThing;
public class Thing : IThing;

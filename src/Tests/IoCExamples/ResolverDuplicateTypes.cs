using AutoCtor;
using System;
using System.Collections.Generic;

// Two registrations that collide with something the provider resolves on its own. Both are
// legal and both win, the way a later registration wins anywhere else: IEnumerable<IPlugin>
// would otherwise be the collection built from the IPlugin registrations, and IServiceProvider
// would otherwise be the provider itself.
//
// A chain of ifs never noticed -- the first branch returned and the rest were unreachable. A
// resolver interface cannot be implemented twice for the same type, so this is what proves
// the built-in resolvers are skipped rather than merely ordered after the registrations.
[ServiceProvider]
[Singleton<IPlugin, FirstPlugin>]
[Singleton<IPlugin, SecondPlugin>]
[Singleton<IEnumerable<IPlugin>, PluginList>]
[Singleton<IServiceProvider, AmbientProvider>]
public sealed partial class DuplicateResolverProvider;

public interface IPlugin;
public class FirstPlugin : IPlugin;
public class SecondPlugin : IPlugin;

public class PluginList : List<IPlugin>;

public class AmbientProvider : IServiceProvider
{
    public object? GetService(Type serviceType) => null;
}

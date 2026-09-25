using AutoCtor;
using System.Collections.Generic;

// The capture is through a collection: one element of IEnumerable<IHook> is scoped, so a
// singleton taking the collection would hold it past the end of every scope.
[ServiceProvider]
[Singleton<IHook, StaticHook>]
[Scoped<IHook, RequestHook>]
[Singleton<IHookRegistry, HookRegistry>]
public partial class HookRegistryProvider;

public interface IHook;
public class StaticHook : IHook;
public class RequestHook : IHook;

public interface IHookRegistry;

public class HookRegistry : IHookRegistry
{
    public HookRegistry(IEnumerable<IHook> hooks) { }
}

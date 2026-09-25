using AutoCtor;

// A transient that reaches a scoped service can only be built inside a scope, so its
// factory lives on the Scope. One that reaches only singletons stays on the provider and
// the scope forwards to it.
[ServiceProvider]
[Singleton<IPlacementConfig, PlacementConfig>]
[Scoped<IPlacementContext, PlacementContext>]
[Transient<IRootSafeCommand, RootSafeCommand>]
[Transient<IScopeBoundCommand, ScopeBoundCommand>]
public partial class PlacementProvider;

public interface IPlacementConfig;
public class PlacementConfig : IPlacementConfig;

public interface IPlacementContext;
public class PlacementContext : IPlacementContext;

public interface IRootSafeCommand;
public class RootSafeCommand : IRootSafeCommand
{
    public RootSafeCommand(IPlacementConfig config) { }
}

public interface IScopeBoundCommand;
public class ScopeBoundCommand : IScopeBoundCommand
{
    public ScopeBoundCommand(IPlacementContext context, IRootSafeCommand command) { }
}

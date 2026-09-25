using AutoCtor;

// AutoCtor emits nothing for an [AutoConstruct] type with no eligible members, so the
// implicit parameterless constructor survives and the emitter must fall back to it.
[ServiceProvider]
[Singleton<IEmptyService, EmptyService>]
public partial class AutoConstructNoMembersProvider;

public interface IEmptyService;

[AutoConstruct]
public partial class EmptyService : IEmptyService;

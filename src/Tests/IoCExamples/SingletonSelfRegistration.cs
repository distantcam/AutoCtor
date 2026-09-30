using AutoCtor;

[ServiceProvider]
[Singleton<SelfService>]
[Singleton(typeof(SelfTypeofService))]
public sealed partial class SelfRegistrationProvider;

public class SelfService;
public class SelfTypeofService;

using AutoCtor;

[ServiceProvider]
[Singleton<ITheService, TheService>]
public sealed partial class GenericAttributeProvider;

public interface ITheService;
public class TheService : ITheService;

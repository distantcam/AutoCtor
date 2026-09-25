using AutoCtor;

[ServiceProvider]
[Singleton<ITheService, TheService>]
public partial class GenericAttributeProvider;

public interface ITheService;
public class TheService : ITheService;

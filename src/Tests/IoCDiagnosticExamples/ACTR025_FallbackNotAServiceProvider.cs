using AutoCtor;

[ServiceProvider(Fallback = nameof(_host))]
[Singleton<IService, Service>]
public partial class WrongFallbackProvider
{
    private readonly string _host = "";
}

public interface IService;
public class Service : IService;

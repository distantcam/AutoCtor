using AutoCtor;

[ServiceProvider]
[Singleton<IUnsealedService, UnsealedService>]
public partial class UnsealedProvider
{
}

public interface IUnsealedService;
public class UnsealedService : IUnsealedService;

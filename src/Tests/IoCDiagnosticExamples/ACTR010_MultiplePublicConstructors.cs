using AutoCtor;

[ServiceProvider]
[Singleton<IMultiCtorService, MultiCtorService>]
public sealed partial class MultiCtorProvider;

public interface IMultiCtorService;

public class MultiCtorService : IMultiCtorService
{
    public MultiCtorService() { }
    public MultiCtorService(int value) { }
}

public interface IServiceA { }
public interface IServiceB { }
public interface IServiceFactory { IServiceB CreateService(); }

[AutoCtor.AutoConstruct]
public partial class PostCtorWithOutArgumentTest
{
    private readonly IServiceA _serviceA;
    private readonly IServiceB _serviceB;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(IServiceFactory serviceFactory, out IServiceB serviceB)
    {
        serviceB = serviceFactory.CreateService();
    }
}

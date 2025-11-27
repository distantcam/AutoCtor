public interface IServiceA { }
public interface IServiceB { }
public interface IServiceFactory { IServiceB CreateService(); }

[AutoCtor.AutoConstruct]
public partial class PostCtorWithRefArgumentTest
{
    private readonly IServiceA _serviceA;
    private readonly IServiceB _serviceB;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(IServiceFactory serviceFactory, ref IServiceB serviceB)
    {
        serviceB = serviceFactory.CreateService();
    }
}

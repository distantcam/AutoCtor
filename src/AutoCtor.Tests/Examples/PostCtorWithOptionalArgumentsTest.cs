public interface IServiceA { }
public interface IServiceB { }

[AutoCtor.AutoConstruct]
public partial class PostCtorWithOptionalArgumentsTest
{
    private readonly IServiceA _serviceA;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(IServiceB serviceB = null)
    {
    }
}

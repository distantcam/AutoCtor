public interface IServiceA { }
public interface IServiceB { }

[AutoCtor.AutoConstruct]
public partial class PostCtorWithArgumentsTest
{
    private readonly IServiceA a;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(IServiceB b)
    {
    }
}

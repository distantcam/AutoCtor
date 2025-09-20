public interface IService { }

public class Service(int value) : IService { public int Value => value; }

[AutoCtor.AutoConstruct]
public partial class PostCtorOptionalInt
{
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(out IService service, int value = 123)
    {
        service = new Service(value);
    }
}

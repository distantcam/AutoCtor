public interface IService { }

public class Service(string? value = null) : IService { public string? Value => value; }

[AutoCtor.AutoConstruct]
public partial class PostCtorOptionalNull
{
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(out IService service, string? value = null)
    {
        service = new Service(value);
    }
}

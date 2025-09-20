public interface IService { }

public class Service(string value) : IService { public string Value => value; }

[AutoCtor.AutoConstruct]
public partial class PostCtorOptionalString
{
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(out IService service, string value = "dfault")
    {
        service = new Service(value);
    }
}

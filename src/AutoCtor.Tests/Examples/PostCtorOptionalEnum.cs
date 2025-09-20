public interface IService { }

public class Service(ConstEnum value) : IService { public ConstEnum Value => value; }

public enum ConstEnum
{
    None,
    Yes,
    No
}

[AutoCtor.AutoConstruct]
public partial class PostCtorOptionalEnum
{
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private void Initialize(out IService service, ConstEnum value = ConstEnum.No)
    {
        service = new Service(value);
    }
}

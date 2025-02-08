public interface IService { }

[AutoCtor.AutoConstruct]
public partial class PostCtorReturnsNonVoidTest
{
    private readonly IService _service;

    [AutoCtor.AutoPostConstruct]
    private System.Threading.Tasks.Task InitAsync() => System.Threading.Tasks.Task.CompletedTask;
}

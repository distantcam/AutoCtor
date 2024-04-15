[AutoCtor.AutoConstruct]
public partial class PostCtorReturnsNonVoidTest
{
    private readonly IA a;

    [AutoCtor.AutoPostConstruct]
    private System.Threading.Tasks.Task InitAsync() => System.Threading.Tasks.Task.CompletedTask;
}

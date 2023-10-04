using System.Threading.Tasks;
using AutoCtor;

[AutoConstruct]
public partial class PostCtorReturnsNonVoidTest
{
    private readonly IA a;

    [AutoPostConstruct]
    private Task InitAsync() => Task.CompletedTask;
}

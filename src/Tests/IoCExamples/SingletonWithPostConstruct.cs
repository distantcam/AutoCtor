using AutoCtor;

// [AutoPostConstruct] adds its own parameters to the generated constructor, so the
// predicted parameter list has to come from the ctor emitter rather than the members.
[ServiceProvider]
[Singleton<IPostDepOne, PostDepOne>]
[Singleton<IPostDepTwo, PostDepTwo>]
[Singleton<IPostService, PostService>]
public sealed partial class PostConstructProvider;

public interface IPostDepOne;
public interface IPostDepTwo;
public class PostDepOne : IPostDepOne;
public class PostDepTwo : IPostDepTwo;

public interface IPostService;

[AutoConstruct]
public partial class PostService : IPostService
{
    private readonly IPostDepOne _one;

    [AutoPostConstruct]
    private void Initialise(IPostDepTwo two) { }
}

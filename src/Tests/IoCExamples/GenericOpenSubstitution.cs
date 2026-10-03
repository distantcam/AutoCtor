using AutoCtor;

// Handler<T> is [AutoConstruct], so its constructor does not exist yet, and its dependency
// mentions T -- the predicted parameter list has to be substituted before it can be used.
[ServiceProvider]
[Singleton(typeof(ISubValidator<>), typeof(SubValidator<>))]
[Singleton(typeof(ISubHandler<>), typeof(SubHandler<>))]
[Singleton<ISubPipeline, SubPipeline>]
public sealed partial class SubstitutionProvider;

public class Invoice;

public interface ISubValidator<T>;
public class SubValidator<T> : ISubValidator<T>;

public interface ISubHandler<T>;

[AutoConstruct]
public partial class SubHandler<T> : ISubHandler<T>
{
    private readonly ISubValidator<T> _validator;
}

public interface ISubPipeline;

public class SubPipeline : ISubPipeline
{
    public SubPipeline(ISubHandler<Invoice> handler) { }
}

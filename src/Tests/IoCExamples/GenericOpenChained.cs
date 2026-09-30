using AutoCtor;

// A construction synthesised from one rule pulls in another, so the closure has to keep
// going rather than stopping after a single pass.
[ServiceProvider]
[Singleton(typeof(IValidator<>), typeof(Validator<>))]
[Singleton(typeof(IHandler<>), typeof(Handler<>))]
[Singleton<IPipeline, Pipeline>]
public sealed partial class PipelineProvider;

public class Command;

public interface IValidator<T>;
public class Validator<T> : IValidator<T>;

public interface IHandler<T>;

public class Handler<T> : IHandler<T>
{
    public Handler(IValidator<T> validator) { }
}

public interface IPipeline;

public class Pipeline : IPipeline
{
    public Pipeline(IHandler<Command> handler) { }
}

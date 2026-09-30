using AutoCtor;

[ServiceProvider]
[Singleton<IInvalidService, InvalidService>]
public sealed partial class GenericProvider<T>
{
}

public interface IInvalidService;
public class InvalidService : IInvalidService;

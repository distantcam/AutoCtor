using AutoCtor;

[ServiceProvider]
[Singleton<IInvalidService, InvalidService>]
public partial class GenericProvider<T>
{
}

public interface IInvalidService;
public class InvalidService : IInvalidService;

using AutoCtor;

public interface IGreeter;
public interface IClock;

public class Clock : IClock;

#region ServiceProvider

[ServiceProvider]
[Singleton<IClock, Clock>]
[Singleton<IGreeter, Greeter>]
public partial class Container;

[AutoConstruct]
public partial class Greeter : IGreeter
{
    private readonly IClock _clock;
}

#endregion

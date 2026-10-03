using AutoCtor;

// The factory replaces the constructor call and nothing else: IClock is still a singleton
// built once on first use, and the factory's parameters are resolved like a constructor's.
[ServiceProvider]
[Singleton<Settings>]
[Singleton<IClock>(Factory = nameof(CreateClock))]
public sealed partial class ClockProvider
{
    private IClock CreateClock(Settings settings) => new UtcClock(settings.Offset);
}

public interface IClock;
public class UtcClock(int offset) : IClock
{
    public int Offset => offset;
}
public class Settings
{
    public int Offset => 0;
}

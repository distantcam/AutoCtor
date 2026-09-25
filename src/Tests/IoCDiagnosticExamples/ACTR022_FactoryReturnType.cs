using AutoCtor;

// The member exists and can be called, but an int is not an IThing.
[ServiceProvider]
[Singleton(typeof(IThing), typeof(Thing), Factory = nameof(CreateNumber))]
public partial class WrongReturnProvider
{
    private int CreateNumber() => 42;
}

public interface IThing;
public class Thing : IThing;

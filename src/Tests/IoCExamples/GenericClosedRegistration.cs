using AutoCtor;

// A fully closed generic is an ordinary registration -- no open generic machinery involved.
[ServiceProvider]
[Singleton<IBox<Widget>, Box<Widget>>]
public sealed partial class ClosedGenericProvider;

public class Widget;
public interface IBox<T>;
public class Box<T> : IBox<T>;

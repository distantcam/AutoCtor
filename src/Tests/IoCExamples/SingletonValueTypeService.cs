using AutoCtor;

// A value typed service has no null to mean "not built yet" and cannot be volatile, so it is
// held in a plain field with a volatile bool beside it. The flag is written after the value,
// which is what publishes it: a reader that sees the flag set also sees the value stored.
[ServiceProvider]
[Singleton(typeof(Stamp), typeof(Stamp))]
[Singleton(typeof(IMarker), typeof(MarkerStruct))]
[Singleton<IPrinter, Printer>]
public partial class ValueTypeProvider;

public struct Stamp;

public interface IMarker;
public struct MarkerStruct : IMarker;

public interface IPrinter;

public class Printer : IPrinter
{
    public Printer(Stamp stamp, IMarker marker) { }
}

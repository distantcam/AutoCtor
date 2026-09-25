using AutoCtor;

[ServiceProvider]
[Singleton<INeedsMissing, NeedsMissing>]
public partial class UnregisteredDependencyProvider;

public interface IMissingService;
public interface INeedsMissing;

public class NeedsMissing : INeedsMissing
{
    public NeedsMissing(IMissingService missing) { }
}

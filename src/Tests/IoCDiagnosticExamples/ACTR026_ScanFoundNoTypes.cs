using AutoCtor;

[ServiceProvider]
[ScanSingleton(typeof(IMissing))]
public partial class EmptyScanProvider;

public interface IMissing;
public abstract class MissingBase : IMissing;

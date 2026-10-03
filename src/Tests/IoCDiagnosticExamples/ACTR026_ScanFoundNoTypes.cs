using AutoCtor;

[ServiceProvider]
[ScanSingleton(typeof(IMissing))]
public sealed partial class EmptyScanProvider;

public interface IMissing;
public abstract class MissingBase : IMissing;

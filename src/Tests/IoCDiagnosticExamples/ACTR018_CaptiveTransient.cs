using AutoCtor;

// The singleton holds the transient for the lifetime of the provider, so ICaptiveWorker
// is only ever constructed once. Still valid code, so the provider is emitted.
//
// The snapshot also covers construction ordering through a transient: the eager assignment
// of ICaptiveHolder calls GetICaptiveWorker(), which reads the ICaptiveSettings field, so
// that field has to be assigned first.
[ServiceProvider]
[Singleton<ICaptiveHolder, CaptiveHolder>]
[Transient<ICaptiveWorker, CaptiveWorker>]
[Singleton<ICaptiveSettings, CaptiveSettings>]
public partial class CaptiveTransientProvider;

public interface ICaptiveSettings;
public class CaptiveSettings : ICaptiveSettings;

public interface ICaptiveWorker;

public class CaptiveWorker : ICaptiveWorker
{
    public CaptiveWorker(ICaptiveSettings settings) { }
}

public interface ICaptiveHolder;

public class CaptiveHolder : ICaptiveHolder
{
    public CaptiveHolder(ICaptiveWorker worker) { }
}

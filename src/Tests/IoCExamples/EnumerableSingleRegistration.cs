using AutoCtor;
using System.Collections.Generic;

// A service registered once is still resolvable as a one element collection.
[ServiceProvider]
[Singleton<ISoleHandler, SoleHandler>]
[Singleton<ISoleConsumer, SoleConsumer>]
public sealed partial class SingleRegistrationProvider;

public interface ISoleHandler;
public class SoleHandler : ISoleHandler;

public interface ISoleConsumer;

public class SoleConsumer : ISoleConsumer
{
    public SoleConsumer(IEnumerable<ISoleHandler> handlers) { }
}

using AutoCtor;
using System.Collections.Generic;

// Nothing is registered for IPlugin, so the collection is empty rather than an error --
// the same as Microsoft's container.
[ServiceProvider]
[Singleton<IPluginHost, PluginHost>]
public partial class EmptyCollectionProvider;

public interface IPlugin;

public interface IPluginHost;

public class PluginHost : IPluginHost
{
    public PluginHost(IEnumerable<IPlugin> plugins) { }
}

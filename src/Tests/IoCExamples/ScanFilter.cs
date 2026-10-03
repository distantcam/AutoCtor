using AutoCtor;
using System.Collections.Generic;

// A scan registers every concrete class it can see that is assignable to the service, in
// name order. Abstract classes, interfaces, generic classes and types the provider cannot
// name are left out.
[ServiceProvider]
[ScanSingleton(typeof(IPlugin))]
[Singleton<IPluginHost, PluginHost>]
public sealed partial class PluginProvider
{
    private class HiddenPlugin : IPlugin;
}

public interface IPlugin;
public interface IExtraPlugin : IPlugin;
public class AudioPlugin : IPlugin;
public class VideoPlugin : IPlugin;
public class ImagePlugin : IExtraPlugin;
public abstract class PluginBase : IPlugin;
public class GenericPlugin<T> : IPlugin;

public interface IPluginHost;

public class PluginHost : IPluginHost
{
    public PluginHost(IEnumerable<IPlugin> plugins) { }
}

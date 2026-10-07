using AutoCtor;

// The two modules import each other. Each is expanded once, and the scan in the inner one
// looks in the module's assembly.
[ServiceProvider]
[Import<OuterModule>]
public sealed partial class NestedModuleProvider;

[Singleton<IPluginHost, PluginHost>]
[Import<InnerModule>]
public class OuterModule;

[ScanSingleton(typeof(IPlugin))]
[Import<OuterModule>]
public class InnerModule;

public interface IPlugin;
public class FirstPlugin : IPlugin;
public class SecondPlugin : IPlugin;

public interface IPluginHost;
public class PluginHost(System.Collections.Generic.IEnumerable<IPlugin> plugins) : IPluginHost
{
    public System.Collections.Generic.IEnumerable<IPlugin> Plugins => plugins;
}

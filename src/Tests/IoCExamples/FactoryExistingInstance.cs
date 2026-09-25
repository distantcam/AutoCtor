using AutoCtor;

// An existing instance is the degenerate factory: a member that takes no arguments and
// hands back something built by hand. A field initializer is one way to fill it; a
// constructor of your own is the other, which FactoryConstructorInstance shows.
[ServiceProvider]
[Singleton<IBanner>(Factory = nameof(_banner))]
public partial class BannerProvider
{
    private readonly IBanner _banner = new Banner("AutoCtor");
}

public interface IBanner;
public class Banner(string text) : IBanner
{
    public string Text => text;
}

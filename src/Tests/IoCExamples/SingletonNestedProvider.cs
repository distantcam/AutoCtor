using AutoCtor;

namespace Nested.Namespace;

public partial class OuterContainer
{
    [ServiceProvider]
    [Singleton<INestedService, NestedService>]
    public sealed partial class InnerProvider;
}

public interface INestedService;
public class NestedService : INestedService;

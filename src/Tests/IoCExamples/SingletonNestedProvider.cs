using AutoCtor;

namespace Nested.Namespace;

public partial class OuterContainer
{
    [ServiceProvider]
    [Singleton<INestedService, NestedService>]
    public partial class InnerProvider;
}

public interface INestedService;
public class NestedService : INestedService;

[AutoCtor.AutoConstruct]
public partial class AmbiguousMarkedMethods
{
    private readonly IA a;

    [AutoCtor.AutoPostConstruct]
    private void Initialize()
    {
    }

    [AutoCtor.AutoPostConstruct]
    private void Initialize(IB b)
    {
    }
}

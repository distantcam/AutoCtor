using AutoCtor;

[AutoConstruct]
public partial class AmbiguousMarkedMethods
{
    private readonly IA a;

    [AutoPostConstruct]
    private void Initialize()
    {
    }

    [AutoPostConstruct]
    private void Initialize(IB b)
    {
    }
}

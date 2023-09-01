using AutoCtor;

[AutoConstruct(nameof(Initialize))]
public partial class AmbiguousInitializeMethods
{
    private readonly IA a;

    private void Initialize()
    {
    }

    private void Initialize(IB b)
    {
    }
}

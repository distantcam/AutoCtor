using AutoCtor;

[assembly: AutoConstruct("Initialize")]

[AutoConstruct]
public partial class AmbiguousInitializerSolvedWithAttributeTest
{
    private readonly IA a;

    [AutoPostConstruct]
    private void Initialize()
    {
    }

    private void Initialize(IB b)
    {
        // This method should be ignored.
    }
}

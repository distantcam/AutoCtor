using AutoCtor;

[assembly: AutoConstruct("Initialize")]

[AutoConstruct]
public partial class AssemblyLevelAttributeTest
{
    private readonly IA a;

    private void Initialize()
    {
    }
}

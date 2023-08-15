using AutoCtor;

[AutoConstruct(nameof(Initialize))]
public partial class ClassWithInitializer
{
    private readonly IA a;

    private void Initialize()
    {
    }
}

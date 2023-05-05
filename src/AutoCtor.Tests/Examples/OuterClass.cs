using AutoCtor;

[AutoConstruct]
public partial class InitializationTest
{
    private readonly string InputName;

    partial void Inititilize()
    {
        NameHash = Encoding.UTF8.GetBytes(InputName);
    }

    public byte[] NameHash { get; }
}

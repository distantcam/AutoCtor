using System.Text;

namespace AutoCtor.Debug;

[AutoConstruct]
public partial class BaseBaseClass
{
    private readonly int _number;
}

[AutoConstruct]
public partial class BaseClass : BaseBaseClass
{
    private readonly string _text;

    partial void Initialize()
    {
        TextBytes = Encoding.UTF8.GetBytes(_text);
    }

    public byte[] TextBytes { get; private set; } = default!;
}

[AutoConstruct]
public partial class TheClass : BaseClass
{
    private readonly bool _flag;
}

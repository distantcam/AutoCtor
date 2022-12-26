using AutoCtor;

[AutoConstruct]
public partial class ExcludeStaticAndInitialisedFieldsTest
{
    private readonly static int S = 4;
    private readonly int _s;
    private const int C = 5;

    private readonly IList<int> _list = new List<int>();
}

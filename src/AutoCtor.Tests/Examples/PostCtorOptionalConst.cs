[AutoCtor.AutoConstruct]
public partial class PostCtorOptionalConst
{
    private const int s_int = 123;
    private const string s_string = "123";

    [AutoCtor.AutoPostConstruct]
    private void Initialize(int num = s_int, string text = s_string)
    {
    }
}

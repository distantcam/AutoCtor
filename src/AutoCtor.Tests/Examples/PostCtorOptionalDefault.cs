public enum Flag
{
    None,
    Rectangular,
    Triangle
}

[AutoCtor.AutoConstruct]
public partial class PostCtorOptionalDefault
{
    [AutoCtor.AutoPostConstruct]
    private void Initialize(string? value = null, Flag flag = default)
    {
    }
}

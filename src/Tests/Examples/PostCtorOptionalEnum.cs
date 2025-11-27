public enum ConstEnum
{
    None,
    Yes,
    No
}

[AutoCtor.AutoConstruct]
public partial class PostCtorOptionalEnum
{
    [AutoCtor.AutoPostConstruct]
    private void Initialize(ConstEnum value = ConstEnum.No)
    {
    }
}

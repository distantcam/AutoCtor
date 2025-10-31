[AutoCtor.AutoConstruct]
public partial class Properties
{
    #region Properties

    // AutoCtor will initialize these
    public string GetProperty { get; }
    protected string ProtectedProperty { get; }
    public string InitProperty { get; init; }
    public required string RequiredProperty { get; set; }

    // AutoCtor will ignore these
    public string InitializerProperty { get; } = "Constant";
    public string GetSetProperty { get; set; }
    public string FixedProperty => "Constant";
    public string RedirectedProperty => InitializerProperty;

    #endregion
}

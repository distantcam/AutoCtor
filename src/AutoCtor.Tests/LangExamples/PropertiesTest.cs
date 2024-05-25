[AutoCtor.AutoConstruct]
public partial class PropertiesTest
{
    public string GetProperty { get; }

    public string InitializerProperty { get; } = "Constant";

    protected string ProtectedProperty { get; }

    public string InitProperty { get; init; }

    public string GetSetProperty { get; set; }

    public string FixedProperty => "Constant";
    public string RedirectedProperty => InitializerProperty;

    [AutoCtor.AutoConstructIgnore]
    public string IgnoredProperty { get; }

#if ROSLYN_4
    public required string RequiredProperty { get; set; }
#endif
}

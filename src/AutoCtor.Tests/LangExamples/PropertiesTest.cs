﻿[AutoCtor.AutoConstruct]
public partial class PropertiesTest
{
    // Included
    public string GetProperty { get; }
    protected string ProtectedProperty { get; }
    public string InitProperty { get; init; }

#if ROSLYN_4
    public required string RequiredProperty { get; set; }
#endif

    // Ignored
    public string InitializerProperty { get; } = "Constant";
    public string GetSetProperty { get; set; }
    public string FixedProperty => "Constant";
    public string RedirectedProperty => InitializerProperty;

    [AutoCtor.AutoConstructIgnore]
    public string IgnoredProperty { get; }

    public string FieldProperty => field ??= "Constant";
}

using System.Diagnostics;

namespace AutoCtor;

public enum GuardSetting
{
    Default,
    Disabled,
    Enabled
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class AutoConstructAttribute : Attribute
{
    public AutoConstructAttribute(GuardSetting guard = GuardSetting.Default)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class AutoPostConstructAttribute : Attribute
{
}

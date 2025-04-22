using System.Diagnostics;
using static System.AttributeTargets;

#pragma warning disable CS9113 // Parameter is unread.

namespace AutoCtor;

public enum GuardSetting
{
    Default,
    Disabled,
    Enabled
}

[AttributeUsage(Class | Struct, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class AutoConstructAttribute(GuardSetting guard = GuardSetting.Default) : Attribute;

[AttributeUsage(Method, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class AutoPostConstructAttribute : Attribute;

[AttributeUsage(Field | Property, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class AutoConstructIgnoreAttribute : Attribute;

[AttributeUsage(Field | Property | Parameter, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class AutoKeyedServiceAttribute(object? key) : Attribute;

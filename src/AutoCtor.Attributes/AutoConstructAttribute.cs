using System.Diagnostics;
using static System.AttributeTargets;

namespace AutoCtor;

public enum GuardSetting
{
    Default,
    Disabled,
    Enabled
}

[AttributeUsage(Class | Struct, AllowMultiple = false, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
#pragma warning disable CS9113 // Parameter is unread.
public sealed class AutoConstructAttribute(GuardSetting guard = GuardSetting.Default) : Attribute;
#pragma warning restore CS9113 // Parameter is unread.

[AttributeUsage(Method, AllowMultiple = false, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class AutoPostConstructAttribute : Attribute;

[AttributeUsage(Field | Property, AllowMultiple = false, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class AutoConstructIgnoreAttribute : Attribute;

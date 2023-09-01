using System.Diagnostics;

namespace AutoCtor;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class AutoConstructAttribute : Attribute
{
    public AutoConstructAttribute()
    {
    }

    public AutoConstructAttribute(string postConstructorMethod)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class AutoPostConstructAttribute : Attribute
{
}

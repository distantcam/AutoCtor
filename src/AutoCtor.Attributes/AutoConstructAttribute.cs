namespace AutoCtor;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
[System.Diagnostics.Conditional("AUTOCTOR_USAGES")]
public sealed class AutoConstructAttribute : Attribute
{
    [System.Runtime.CompilerServices.CompilerGenerated]
    public AutoConstructAttribute()
    {
    }
}

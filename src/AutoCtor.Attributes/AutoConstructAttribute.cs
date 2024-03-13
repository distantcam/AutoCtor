using System.Diagnostics;

namespace AutoCtor;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class AutoConstructAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
[Conditional("AUTOCTOR_USAGES")]
public sealed class AutoPostConstructAttribute : Attribute;

using System.ComponentModel;

// https://github.com/dotnet/roslyn/issues/45510

namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IsExternalInit
{
}

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

#if ROSLYN_3
using EmitterContext = Microsoft.CodeAnalysis.GeneratorExecutionContext;
#elif ROSLYN_4
using EmitterContext = Microsoft.CodeAnalysis.SourceProductionContext;
#endif

internal static class Extensions
{
    public static T? OnlyOrDefault<T>(this IEnumerable<T> source)
    {
        if (source is IList<T> list)
        {
            return list.Count switch
            {
                1 => list[0],
                _ => default,
            };
        }
        using var e = source.GetEnumerator();
        if (!e.MoveNext()) return default;
        var result = e.Current;
        if (!e.MoveNext()) return result;
        return default;
    }

    public static T? OnlyOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        using var e = source.GetEnumerator();
        T? result = default;
        do
        {
            if (!e.MoveNext()) return result;
        } while (!predicate(e.Current));
        result = e.Current;
        do
        {
            if (!e.MoveNext()) return result;
        } while (!predicate(e.Current));
        return default;
    }

    [SuppressMessage(
        "MicrosoftCodeAnalysisCorrectness",
        "RS1035:Do not use APIs banned for analyzers",
        Justification = "Old generator still maintained")]
    public static void ReportDiagnostic(this EmitterContext context, IHaveDiagnostics item, DiagnosticDescriptor diagnostic)
    {
        foreach (var loc in item.Locations)
            context.ReportDiagnostic(Diagnostic.Create(diagnostic, loc, item.ErrorName));
    }
}

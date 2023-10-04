using AutoSource;

namespace AutoCtor;

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
        using (var e = source.GetEnumerator())
        {
            if (!e.MoveNext()) return default;
            var result = e.Current;
            if (!e.MoveNext()) return result;
            return default;
        }
    }

    public static T? OnlyOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        using (var e = source.GetEnumerator())
        {
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
    }

    public static bool MoreThan<T>(this IEnumerable<T> source, int limit)
    {
        if (source is ICollection<T> collection)
            return collection.Count > limit;
        var count = 0;
        using (var e = source.GetEnumerator())
        {
            while (e.MoveNext())
            {
                count++;
                if (count > limit)
                    return true;
            }
        }
        return false;
    }

    public static IDisposable StartPartialType(this CodeBuilder code, string? ns, IReadOnlyList<string> typeDeclarations)
    {
        if (!string.IsNullOrEmpty(ns))
        {
            code.AppendLine($"namespace {ns}");
            code.StartBlock();
        }

        for (var i = 0; i < typeDeclarations.Count; i++)
        {
            code.AppendLine(typeDeclarations[i]);
            code.StartBlock();
        }

        return new CloseBlock(code, typeDeclarations.Count + (ns != null ? 1 : 0));
    }

    private readonly struct CloseBlock : IDisposable
    {
        private readonly CodeBuilder _codeBuilder;
        private readonly int _count;
        public CloseBlock(CodeBuilder codeBuilder, int count) { _codeBuilder = codeBuilder; _count = count; }
        public void Dispose() { for (var i = 0; i < _count; i++) _codeBuilder.EndBlock(); }
    }
}

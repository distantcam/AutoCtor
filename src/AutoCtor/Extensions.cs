namespace AutoCtor;

internal static class Extensions
{
    public static T? OnlyOrDefault<T>(this IEnumerable<T> source)
    {
        if (source is IList<T> list)
        {
            switch (list.Count)
            {
                case 1: return list[0];
                default: return default;
            }
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
        var result = default(T);
        long count = 0;
        foreach (var element in source)
        {
            if (predicate(element))
            {
                result = element;
                checked { count++; }
            }
        }
        return count switch
        {
            1 => result,
            _ => default,
        };
    }
}

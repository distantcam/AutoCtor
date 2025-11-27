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

    public static bool MoreThan<T>(this IEnumerable<T> source, int limit)
    {
        if (source is ICollection<T> collection)
            return collection.Count > limit;
        var count = 0;
        using var e = source.GetEnumerator();
        while (e.MoveNext())
        {
            count++;
            if (count > limit)
                return true;
        }
        return false;
    }
}

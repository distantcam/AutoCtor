using System.Collections;
using System.Collections.Immutable;

namespace AutoCtor.Models;

internal struct EquatableList<T> : IEquatable<EquatableList<T>>, IReadOnlyList<T>, IEnumerable<T>
{
    private readonly ImmutableArray<T> _data;

    public EquatableList(IEnumerable<T> data)
    {
        _data = data.ToImmutableArray();
    }

    public bool Equals(EquatableList<T> other)
    {
        return _data.Length == other._data.Length && _data
            .Zip(other._data, (x, y) => (First: x, Second: y))
            .All(x => EqualityComparer<T>.Default.Equals(x.First, x.Second));
    }

    public override bool Equals(object obj)
    {
        if (obj is not EquatableList<T> el) return false;
        return Equals(el);
    }

    public override int GetHashCode() =>
        _data.Aggregate(0, (a, n) => a * 0x29_55_55_A5 + EqualityComparer<T>.Default.GetHashCode(n));

    public readonly int Count => _data.Length;
    public readonly T this[int index] => _data[index];

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_data).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_data).GetEnumerator();
}

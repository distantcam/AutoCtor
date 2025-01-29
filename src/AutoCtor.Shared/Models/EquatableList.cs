using System.Collections;
using System.Collections.Immutable;

internal readonly struct EquatableList<T> : IEquatable<EquatableList<T>>, IReadOnlyList<T>, IEnumerable<T>
{
    private readonly IEqualityComparer<T> _equalityComparer;
    private readonly ImmutableArray<T> _data = ImmutableArray<T>.Empty;
    private readonly int _hash = 0;

    public EquatableList(IEnumerable<T> data, IEqualityComparer<T>? equalityComparer = null)
    {
        _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        _data = data.ToImmutableArray();
        for (var i = 0; i < _data.Length; i++)
            _hash = _hash * 0x29_55_55_A5 + _equalityComparer.GetHashCode(_data[i]);
    }

    public bool Equals(EquatableList<T> other)
    {
        if (_data.IsDefault != other._data.IsDefault)
            return false;
        if (_data.IsDefault)
            return true;

        if (_data.Length != other._data.Length)
            return false;

        var thisEnumerator = _data.GetEnumerator();
        var otherEnumerator = other._data.GetEnumerator();

        while (thisEnumerator.MoveNext())
        {
            if (!otherEnumerator.MoveNext() || !_equalityComparer.Equals(thisEnumerator.Current, otherEnumerator.Current))
                return false;
        }

        return !otherEnumerator.MoveNext();
    }

    public override bool Equals(object obj)
    {
        if (obj is not EquatableList<T> el) return false;
        return Equals(el);
    }

    public override int GetHashCode() => _hash;

    public readonly int Count => _data.Length;
    public readonly T this[int index] => _data[index];

    public IEnumerator<T> GetEnumerator() => _data.IsDefaultOrEmpty
        ? Enumerable.Empty<T>().GetEnumerator()
        : ((IEnumerable<T>)_data).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

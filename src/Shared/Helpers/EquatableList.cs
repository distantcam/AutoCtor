using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

[DebuggerTypeProxy(typeof(EquatableList<>.EquatableListDebugView))]
[DebuggerDisplay("Count = {Count}")]
internal readonly struct EquatableList<T> : IEquatable<EquatableList<T>>, IReadOnlyList<T>
{
    private readonly ImmutableArray<T> _data = ImmutableArray<T>.Empty;
    private readonly int _hash = 0;

    public EquatableList(IEnumerable<T> data) : this(data.ToImmutableArray())
    {
    }

    // Taking the array itself lets a caller that already knows the count hand over a
    // right-sized one instead of having it rebuilt through IEnumerable.
    public EquatableList(ImmutableArray<T> data)
    {
        _data = data;
        for (var i = 0; i < _data.Length; i++)
            _hash = _hash * 0x29_55_55_A5 + EqualityComparer<T>.Default.GetHashCode(_data[i]);
    }

    public bool Equals(EquatableList<T> other)
    {
        if (_data.IsDefault || other._data.IsDefault)
            return _data.IsDefault == other._data.IsDefault;

        if (_hash != other._hash || _data.Length != other._data.Length)
            return false;

        for (var i = 0; i < _data.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(_data[i], other._data[i]))
                return false;
        }

        return true;
    }

    public override bool Equals(object obj)
    {
        if (obj is not EquatableList<T> el) return false;
        return Equals(el);
    }

    public override int GetHashCode() => _hash;

    public readonly int Count => _data.Length;
    public readonly T this[int index] => _data[index];

    public ImmutableArray<T>.Enumerator GetEnumerator() => Data.GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)Data).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)Data).GetEnumerator();

    private ImmutableArray<T> Data => _data.IsDefault ? ImmutableArray<T>.Empty : _data;

    internal sealed class EquatableListDebugView(EquatableList<T> equatableList)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var items = new T[equatableList.Count];
                equatableList._data.CopyTo(items, 0);
                return items;
            }
        }
    }
}

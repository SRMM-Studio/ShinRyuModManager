using System.Collections;

namespace Utils;

// Note: This class is very slow at removes (O(n)). Minimize the need to remove values
public class OrderedSet<T> : ICollection<T> where T : notnull
{
    // We don't care about the value
    private readonly OrderedDictionary<T, byte> _dictionary;

    public OrderedSet() : this(EqualityComparer<T>.Default) { }

    public OrderedSet(IEqualityComparer<T> comparer)
    {
        _dictionary = new OrderedDictionary<T, byte>(comparer);
    }

    public int Count => _dictionary.Count;

    public bool IsReadOnly => false;

    void ICollection<T>.Add(T item) => Add(item);

    public bool Add(T item)
    {
        return _dictionary.TryAdd(item, 0);
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public bool Remove(T item)
    {
        return _dictionary.Remove(item);
    }

    public void UnionWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        foreach (var item in other)
        {
            Add(item);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var kvp in _dictionary)
        {
            yield return kvp.Key;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Contains(T item)
    {
        return _dictionary.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);

        foreach (var key in _dictionary.Keys)
        {
            array[arrayIndex++] = key;
        }
    }
}
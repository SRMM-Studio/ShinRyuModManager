using System.Collections;

namespace Utils;

public class OrderedSet<T> : ICollection<T>
{
    private readonly IDictionary<T, LinkedListNode<T>> _dictionary;
    private readonly LinkedList<T> _linkedList;
    
    public OrderedSet() : this(EqualityComparer<T>.Default) { }
    
    public OrderedSet(IEqualityComparer<T> comparer)
    {
        _dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
        _linkedList = new LinkedList<T>();
    }
    
    public int Count
    {
        get => _dictionary.Count;
    }
    
    public bool IsReadOnly
    {
        get => _dictionary.IsReadOnly;
    }
    
    void ICollection<T>.Add(T item)
    {
        Add(item);
    }
    
    public bool Add(T item)
    {
        if (_dictionary.ContainsKey(item))
            return false;
        
        var node = _linkedList.AddLast(item);
        _dictionary.Add(item, node);
        
        return true;
    }
    
    public void Clear()
    {
        _linkedList.Clear();
        _dictionary.Clear();
    }
    
    public bool Remove(T item)
    {
        if (item is null)
            return false;
        
        var found = _dictionary.TryGetValue(item, out var node);
        
        if (!found)
            return false;
        
        _dictionary.Remove(item);
        _linkedList.Remove(node);
        
        return true;
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
        return _linkedList.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public bool Contains(T item)
    {
        if (item is null)
            return false;
        
        return _dictionary.ContainsKey(item);
    }
    
    public void CopyTo(T[] array, int arrayIndex)
    {
        _linkedList.CopyTo(array, arrayIndex);
    }
}

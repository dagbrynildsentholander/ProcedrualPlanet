using System.Collections.Concurrent;

public class MeshDataCache<A, B>
{
    private ConcurrentDictionary<A, B> dictionary;
    private int maxSize;

    public MeshDataCache(int maxSize)
    {
        dictionary = new ConcurrentDictionary<A, B>();
        this.maxSize = maxSize;
    }

    public MeshDataCache()
    {
        dictionary = new ConcurrentDictionary<A, B>();
        this.maxSize = 1000;
    }

    public bool TryAdd(A key, B value)
    {
        if (dictionary.Count >= maxSize)
        {
            dictionary.Clear();
        }

        return dictionary.TryAdd(key, value);

    }
    public bool ContainsKey(A key)
    {
        return dictionary.ContainsKey(key);
    }
    public bool TryGetValue(A key, out B value)
    {
        return dictionary.TryGetValue(key, out value);
    }
    public int Count
    {
        get { return dictionary.Count; }
    }
}

namespace Coursework_paralel_computing.TechClasses
{
    internal interface IMyConcurrentDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        bool TryAdd(TKey key, TValue value);
        bool TryRemove(TKey key, out TValue value);
    }
}

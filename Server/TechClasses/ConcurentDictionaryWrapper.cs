using System.Collections;
using System.Collections.Concurrent;

namespace Coursework_paralel_computing.TechClasses
{
    internal class ConcurentDictionaryWrapper<TKey, TValue> : IMyConcurrentDictionary<TKey, TValue>
    {
        ConcurrentDictionary<TKey, TValue> _dict;
        public ConcurentDictionaryWrapper()
        {

            _dict = new ConcurrentDictionary<TKey, TValue>();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        public bool TryAdd(TKey key, TValue value)
        {
            return _dict.TryAdd(key, value);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            return _dict.TryRemove(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }
    }
}

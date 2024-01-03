using System.Collections;
namespace Coursework_paralel_computing.TechClasses
{
    internal class MyConcurrentDictionary<TKey, TValue> : IMyConcurrentDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _dictionary;
        private ReaderWriterLockSlim _lockSlim;

        public MyConcurrentDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
            _lockSlim = new ReaderWriterLockSlim();
        }

        public bool TryAdd(TKey key, TValue value)
        {
            try
            {
                _lockSlim.EnterWriteLock();
                if (!_dictionary.ContainsKey(key))
                {
                    _dictionary.Add(key, value);
                    return true;
                }
                return false;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            try
            {
                _lockSlim.EnterWriteLock();
                if (_dictionary.TryGetValue(key, out value))
                {
                    _dictionary.Remove(key);
                    return true;
                }
                return false;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            try
            {
                _lockSlim.EnterReadLock();
                foreach (var pair in _dictionary)
                {
                    yield return pair;
                }
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

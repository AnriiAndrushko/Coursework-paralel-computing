using System.Collections.Concurrent;

namespace InvertedIndexLib.TechClasses
{
    internal class ConcurrentQueueWrapper<TValue> : IMyConcurrentQueue<TValue>
    {
        public int Count { get => _queue.Count; }
        ConcurrentQueue<TValue> _queue;
        public ConcurrentQueueWrapper()
        {
            _queue = new ConcurrentQueue<TValue>();
        }

        public void Enqueue(TValue value)
        {
            _queue.Enqueue(value);
        }

        public bool TryDequeue(out TValue value)
        {
            return _queue.TryDequeue(out value);
        }

    }
}

namespace InvertedIndexLib.TechClasses
{
    internal class MyConcurrentQueue<TValue> : IMyConcurrentQueue<TValue>
    {
        private Queue<TValue> queue = new Queue<TValue>();
        private object syncRoot = new object();

        public int Count
        {
            get
            {
                lock (syncRoot)
                {
                    return queue.Count;
                }
            }
        }

        public bool TryDequeue(out TValue value)
        {
            lock (syncRoot)
            {
                if (queue.Count > 0)
                {
                    value = queue.Dequeue();
                    return true;
                }
                else
                {
                    value = default;
                    return false;
                }
            }
        }

        public void Enqueue(TValue value)
        {
            lock (syncRoot)
            {
                queue.Enqueue(value);
            }
        }
    }
}

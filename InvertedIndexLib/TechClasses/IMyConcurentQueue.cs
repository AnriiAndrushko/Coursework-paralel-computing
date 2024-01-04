namespace InvertedIndexLib.TechClasses
{
    internal interface IMyConcurrentQueue<TValue>
    {
        public int Count { get; }
        bool TryDequeue(out TValue value);
        void Enqueue(TValue value);
    }
}

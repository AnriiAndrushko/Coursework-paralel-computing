using System.Collections.Concurrent;

namespace InvertedIndexLib
{
    public class InvertedIndexKeeper:IDisposable
    {
        MyThreadPool _pool;
        ConcurrentQueue<Task> _queue;
        InvertedIndex _index;
        public bool IsBusy => _pool.IsBusy;

        public InvertedIndexKeeper(int threadCount)
        {
            _queue = new ConcurrentQueue<Task>();
            _pool = new MyThreadPool(_queue, threadCount);
            _index = new InvertedIndex();
            _pool.Start();
        }

        public void Save(string path)
        {
            _pool.Stop();
            _index.SaveIndex(path);
            _pool.Start();
        }

        public void Load(string path)
        {
            _pool.Stop();
            _index = InvertedIndex.LoadIndex(path);
            _pool.Start();
        }

        public void AddDoc(string docPath)
        {
            _queue.Enqueue(new Task(() => _index.AddDoc(docPath)));
        }
        public void AddText(string text, string textOrigin = "Debug")
        {
            _queue.Enqueue(new Task(() => _index.AddText(text, textOrigin)));
        }

        public IEnumerable<string> GetByWord(string word)
        {
            var tmp = new Task<IEnumerable<string>>(() => _index.GetByWord(word));
            _queue.Enqueue(tmp);
            return tmp.Result;
        }

        public IEnumerable<string> GetByQuery(string query)
        {
            var tmp = new Task<IEnumerable<string>>(() => _index.GetByQuery(query));
            _queue.Enqueue(tmp);
            return tmp.Result;
        }

        public void Dispose()
        {
            _pool.Stop();
        }
    }
}

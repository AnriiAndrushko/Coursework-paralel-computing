using System.Collections.Concurrent;

namespace InvertedIndexLib
{
    public class InvertedIndexKeeper
    {
        MyThreadPool _pool;
        ConcurrentQueue<Task> _queue;
        InvertedIndex _index;

        public InvertedIndexKeeper(int threadCount)
        {
            _queue = new ConcurrentQueue<Task>();
            _pool = new MyThreadPool(_queue, threadCount);
            _index = new InvertedIndex();
            _pool.Start();
        }

        public void Save(string path)
        {
            _index.SaveIndex(path);
        }

        public void Load(string path)
        {
            _pool.Stop();
            _index = InvertedIndex.LoadIndex(path);
            _pool.Start();
        }

        public void AddDoc(string docPath)
        {
            _queue.Append(new Task(() => _index.AddDoc(docPath)));
        }

        public IEnumerable<string> GetByWord(string word)
        {
            var tmp = new Task<IEnumerable<string>>(() => _index.GetByWord(word));
            _queue.Append(tmp);
            return tmp.Result;
        }

        public IEnumerable<string> GetByQuery(string sentence)
        {
            var tmp = new Task<IEnumerable<string>>(() => _index.GetByQuery(sentence));
            _queue.Append(tmp);
            return tmp.Result;
        }
    }
}

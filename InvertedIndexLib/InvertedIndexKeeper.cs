﻿using InvertedIndexLib.TechClasses;
using System.Collections.Concurrent;

namespace InvertedIndexLib
{
    public class InvertedIndexKeeper:IDisposable
    {
        private MyThreadPool _pool;
        private IMyConcurrentQueue<Task> _queue;
        private ManualResetEventSlim _taskAvailableEvent;
        private InvertedIndex _index;
        public event NotifyCompleted TasksCompleted;


        public InvertedIndexKeeper(int threadCount, bool useOwnQueue = false)
        {
            _taskAvailableEvent = new ManualResetEventSlim(false);
            _queue = useOwnQueue? new MyConcurrentQueue<Task>():new ConcurrentQueueWrapper<Task>();
            _pool = new MyThreadPool(_queue, _taskAvailableEvent, threadCount);
            _index = new InvertedIndex();
            _pool.Start();
            _pool.TasksCompleted += OnTasksCompleted;
        }
        private void OnTasksCompleted()
        {
            TasksCompleted?.Invoke();
        }

        public void Save(string path)
        {
            _pool.Stop();
            _index.SaveIndex(path);
            _pool.Start();
        }

        public void Load(string path)
        {
            if (!File.Exists(path)) { return; }
            _pool.Stop();
            _index = InvertedIndex.LoadIndex(path);
            _pool.Start();
        }

        public void AddDoc(string docPath)
        {
            _queue.Enqueue(new Task(() => _index.AddDoc(docPath)));
            _taskAvailableEvent.Set();
        }
        public void AddText(string text, string textOrigin = "Debug")
        {
            _queue.Enqueue(new Task(() => _index.AddText(text, textOrigin)));
            _taskAvailableEvent.Set();
        }

        public IEnumerable<string> GetByWord(string word)
        {
            var tmp = new Task<IEnumerable<string>>(() => _index.GetByWord(word));
            _queue.Enqueue(tmp);
            _taskAvailableEvent.Set();
            return tmp.Result;
        }

        public IEnumerable<string> GetByQuery(string query)
        {
            var tmp = new Task<IEnumerable<string>>(() => _index.GetByQuery(query));
            _queue.Enqueue(tmp);
            _taskAvailableEvent.Set();
            return tmp.Result;
        }

        public void Dispose()
        {
            _pool.Stop();
        }
    }
}

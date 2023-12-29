using System.Collections.Concurrent;

namespace InvertedIndexLib
{
    internal class MyThreadPool
    {
        Thread[] _pool;
        ConcurrentQueue<Task> _taskQueue;
        CancellationTokenSource _cts;
        int _busyCounter;
        object _busyCounterLock;
        private readonly int _size;
        public bool IsBusy { get { lock (_busyCounterLock) { return _busyCounter != 0; } } }
        public int Size => _size;

        public MyThreadPool(ConcurrentQueue<Task> taskQueue, int size = 6)
        {
            _busyCounterLock = new object();
            _busyCounter = 0;
            _size = size;
            _cts = new CancellationTokenSource();
            _taskQueue = taskQueue;
            _pool = new Thread[size];
            for (int i = 0; i < _pool.Length; i++)
            {
                _pool[i] = new Thread(new ThreadStart(worker));
            }
        }

        public void Start()
        {
            for (int i = 0; i < _pool.Length; i++)
            {
                _pool[i].Start();
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            for (int i = 0; i < _pool.Length; i++)
            {
                _pool[i].Join();
            }
        }

        private void worker()
        {
            Task curTask;
            while (!_cts.Token.IsCancellationRequested)
            {
                if (!_taskQueue.IsEmpty)//TODO: maybe use semafor here
                {
                    if (_taskQueue.TryDequeue(out curTask))
                    {
                        lock (_busyCounterLock)
                        {
                            _busyCounter++;
                        }
                        curTask.RunSynchronously();
                        lock (_busyCounterLock)
                        {
                            _busyCounter--;
                        }
                    }
                }
            }
        }
    }
}

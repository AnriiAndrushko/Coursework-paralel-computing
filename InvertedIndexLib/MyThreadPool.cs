using System.Collections.Concurrent;
using System.Threading;

namespace InvertedIndexLib
{
    public delegate void NotifyCompleted();
    internal class MyThreadPool
    {
        private Thread[] _pool;
        private ConcurrentQueue<Task> _taskQueue;
        static ManualResetEventSlim _taskAvailableEvent;
        private CancellationTokenSource _cts;
        private int _busyCounter;
        private object _busyCounterLock;
        private readonly int _size;
        public bool IsBusy { get { lock (_busyCounterLock) { return _busyCounter != 0; } } }
        public int Size => _size;
        public event NotifyCompleted TasksCompleted;

        public MyThreadPool(ConcurrentQueue<Task> taskQueue, ManualResetEventSlim taskAvailableEvent, int size = 6)
        {
            _busyCounterLock = new object();
            _busyCounter = 0;
            _size = size;
            _taskQueue = taskQueue;
            _pool = new Thread[size];
            _taskAvailableEvent = taskAvailableEvent;
        }

        public void Start()
        {
            if (_cts != null) _cts.Dispose();
            _cts = new CancellationTokenSource();

            for (int i = 0; i < _pool.Length; i++)
            {
                _pool[i] = new Thread(new ThreadStart(worker));
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
                try { _taskAvailableEvent.Wait(_cts.Token); }
                catch { return; }
                
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
                        if (_busyCounter == 0 && _taskQueue.Count == 0)
                        {
                            TasksCompleted?.Invoke();
                        }
                    }
                    
                }
                else
                {
                    _taskAvailableEvent.Reset();
                }
            }
        }
    }
}

using InvertedIndexLib.TechClasses;

namespace Tests
{
    [TestClass]
    public class ThreadPoolTest
    {
        ManualResetEvent isDoneMarker;
        [TestMethod]
        public void ThreadPool_CreatesNewThreadOwnQueue()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            int actualThreadId = -1;
            isDoneMarker = new ManualResetEvent(false);


            MyConcurrentQueue<Task> queue = new MyConcurrentQueue<Task>();
            MyThreadPool pool = new MyThreadPool(queue, new ManualResetEventSlim(true),2);
            pool.TasksCompleted += done;

            queue.Enqueue(new Task(() =>
            {
                actualThreadId = Thread.CurrentThread.ManagedThreadId;
            }));
            pool.Start();

            isDoneMarker.WaitOne();
            Assert.AreNotEqual(currentThreadId, actualThreadId);
            Assert.AreNotEqual(-1, actualThreadId);
        }
        void done()
        {
            isDoneMarker.Set();
        }

        [TestMethod]
        public void ThreadPool_CreatesNewThreadBuildInQueue()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            int actualThreadId = -1;
            isDoneMarker = new ManualResetEvent(false);

            ConcurrentQueueWrapper<Task> queue = new ConcurrentQueueWrapper<Task>();
            MyThreadPool pool = new MyThreadPool(queue, new ManualResetEventSlim(true), 2);
            pool.TasksCompleted += done;

            queue.Enqueue(new Task(() =>
            {
                actualThreadId = Thread.CurrentThread.ManagedThreadId;
            }));
            pool.Start();

            isDoneMarker.WaitOne();
            //pool.Stop();
            Assert.AreNotEqual(currentThreadId, actualThreadId);
            Assert.AreNotEqual(-1, actualThreadId);
        }
    }
}

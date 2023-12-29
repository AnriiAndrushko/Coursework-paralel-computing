using System.Collections.Concurrent;

namespace Tests
{
    [TestClass]
    public class ThreadPoolTest
    {
        [TestMethod]
        public void ThreadPool_QueueUserWorkItem_Success()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;
            int actualThreadId = -1;

            ConcurrentQueue<Task> queue = new ConcurrentQueue<Task>();
            MyThreadPool pool = new MyThreadPool(queue, 2);

            queue.Enqueue(new Task(() =>
            {
                actualThreadId = Thread.CurrentThread.ManagedThreadId;
            }));
            pool.Start();

            Thread.Sleep(100);

            Assert.AreNotEqual(currentThreadId, actualThreadId);
            Assert.AreNotEqual(-1, actualThreadId);
        }
    }
}

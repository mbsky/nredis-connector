namespace for_profiler
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using Connector;

    #endregion

    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            var host = "localhost";

            Profile(new NormalCommandFactoryPool(host, 6379), "Normal");
            Profile(new PipelinedCommandFactoryPool(host, 6379, 1), "Pipelined 1 conn");
            Profile(new PipelinedCommandFactoryPool(host, 6379, 2), "Pipelined 2 conn");
            Profile(new PipelinedCommandFactoryPool(host, 6379, 5), "Pipelined 5 conn");
            Profile(new PipelinedCommandFactoryPool(host, 6379, 10), "Pipelined 10 conn");
            Profile(new PipelinedCommandFactoryPool(host, 6379, 50), "Pipelined 50 conn");
        }

        private static void Profile(ICommandFactoryPool pool, string testName)
        {
            long counter = 0;
            bool run = true;
            var evt = new ManualResetEvent(false);

            var ts = new ParameterizedThreadStart(
                (oPool) =>
                    {
                        var factory = ((ICommandFactoryPool)oPool).Get();
                        var cmdSet = factory.Set("foo", "bar");
                        var cmdGet = factory.Get("foo");
                        evt.WaitOne();
                        while (run)
                        {
                            cmdSet.Exec();
                            cmdGet.Exec();
                            Interlocked.Increment(ref counter);
                        }
                    });

            var workers = new List<Thread>();
            for (int i = 0; i < 50; i++)
            {
                var t = new Thread(ts);
                t.Start(pool);
                workers.Add(t);
            }
            var sw = new Stopwatch();
            sw.Start();
            evt.Set();
            for (int i = 0; i < 5; i++ )
            {
                Thread.Sleep(1000);
                Console.Write(".");
            }
                run = false;
            sw.Stop();
            foreach (var t in workers)
            {
                t.Join();
            }
            pool.Dispose();
            Console.WriteLine("{0}\t:{1}", testName, counter * 2000.0 / sw.ElapsedMilliseconds);
        }

        #endregion
    }
}
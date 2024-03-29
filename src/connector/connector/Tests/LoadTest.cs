﻿namespace Connector.Tests
{
    using System;
    using System.Diagnostics;

    using NUnit.Framework;
    using System.Threading;

    [TestFixture]
    public class LoadTest
    {
        private Process _redisProc;

        const int TestTimeMs = 10000;

        private const string _host = "localhost";

        [SetUp]
        public void Setup()
        {
            this._redisProc = Process.Start(
                new ProcessStartInfo()
                {
                    FileName = @"..\..\..\..\..\lib\redis\redis-server.exe",
                });

            if (this._redisProc == null)
            {
                throw new Exception("Unable to start redis");
            }

        }

        [Test, Ignore]
        public void PoolPerformanceCompare()
        {
                System.Diagnostics.Stopwatch sw1 = new Stopwatch();
                
                int count = 0;
                sw1.Start();
                while (sw1.ElapsedMilliseconds < TestTimeMs)
                {
                    using (var conn = RedisConnection.Connect(_host, 6379))
                    {
                        var f = new CommandFactory(new NormalCommandExecutor(conn));
                        f.Set(Guid.NewGuid().ToString(), "bar").Exec();
                        count++;
                    }
                }
                sw1.Stop();

                System.Diagnostics.Stopwatch sw2 = new Stopwatch();
                
                int count2 = 0;
                sw2.Start();
                using(var pool = new ConnectionPool(_host, 6379))
                {
                    while (sw2.ElapsedMilliseconds < TestTimeMs)
                    {
                        using (var conn = pool.GetConnection())
                        {
                            var f = new CommandFactory(new NormalCommandExecutor(conn));
                            f.Set(Guid.NewGuid().ToString(), "bar").Exec();
                            count2++;
                        }
                    }
                }
                sw2.Stop();

                Assert.Fail(String.Format("Pooled: {0} Sets/Sec, NotPooled: {1} Sets/Sec", 
                    count2 * 1000.0 / sw2.ElapsedMilliseconds,
                    count * 1000.0 / sw1.ElapsedMilliseconds));
        }

        [Test, Ignore]
        public void SetsPerSecond()
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            using (var conn = RedisConnection.Connect(_host, 6379))
            {
                var f = new CommandFactory(new NormalCommandExecutor(conn));
                int count = 0;
                sw.Start();
                
                while (sw.ElapsedMilliseconds < TestTimeMs)
                {
                    f.Set(Guid.NewGuid().ToString(), "bar").Exec();
                    count++;
                }
                sw.Stop();

                Assert.Fail(String.Format("{0} Sets/Sec", count * 1000.0 / sw.ElapsedMilliseconds));

            }
        }

        [Test, Ignore]
        public void SetsPerSecondWith50ThreadsNormal()
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            long counter = 0;
            bool run = true;
            var evt = new ManualResetEvent(false);
            
            var ts = new ParameterizedThreadStart((o) =>
            {
                using (var conn = RedisConnection.Connect(_host, 6379))
                {
                    var f = new CommandFactory(new NormalCommandExecutor(conn));
                    evt.WaitOne();
                    while (run)
                    {
                        f.Set("bar", "baz").Exec();
                        Interlocked.Increment(ref counter);
                    }
                }

            });
            var workers = new System.Collections.Generic.List<Thread>();
            for (int i = 0; i < 50; i++)
            {
                var t = new Thread(ts);
                t.Start(null);
                workers.Add(t);
            }
            sw.Start();
            evt.Set();
            Thread.Sleep(30000);
            run = false;
            sw.Stop();
            foreach (var t in workers)
            {
                t.Join();
            }
            Assert.Pass(String.Format("{0} Sets/Sec", counter * 1000.0 / sw.ElapsedMilliseconds));

        }

        [Test]
        public void SetsPerSecondWith50ThreadsPipelined()
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            long counter = 0;
            bool run = true;
            var evt = new ManualResetEvent(false);
            using (var pool = new PipelinedCommandFactoryPool(_host, 6379))
            {
                var ts = new ThreadStart(() =>
                {
                    var f = pool.Get();
                    evt.WaitOne();
                    while (run)
                    {
                        f.Set("bar", "baz").Exec();
                        Interlocked.Increment(ref counter);
                    }

                });
                var workers = new System.Collections.Generic.List<Thread>();
                for (int i = 0; i < 50; i++)
                {
                    var t = new Thread(ts);
                    t.Start();
                    workers.Add(t);
                }
                sw.Start();
                evt.Set();
                Thread.Sleep(30000);
                run = false;
                sw.Stop();
                foreach (var t in workers)
                {
                    t.Join();
                }
            }
            Assert.Pass(String.Format("{0} Sets/Sec", counter * 1000.0 / sw.ElapsedMilliseconds));
            
        }

        [Test, Ignore]
        public void GetsPerSecond()
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            using (var conn = RedisConnection.Connect(_host, 6379))
            {
                var f = new CommandFactory(new NormalCommandExecutor(conn));
                int count = 0;
                f.Set("foo", "bar").Exec();

                sw.Start();
                while (sw.ElapsedMilliseconds < TestTimeMs)
                {
                    RedisCommandWithBytes bytes = f.Get("foo");
                    bytes.Exec();
                    count++;
                }
                sw.Stop();

                Assert.Pass(String.Format("{0} Sets/Sec", count * 1000.0 / sw.ElapsedMilliseconds));

            }
        }
        [Test, Ignore]
        public void RPushPerSecond()
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            using (var conn = RedisConnection.Connect(_host, 6379))
            {
                var f = new CommandFactory(new NormalCommandExecutor(conn));
                int count = 0;
                
                var bytes = Guid.NewGuid().ToByteArray();
                sw.Start();
                while (sw.ElapsedMilliseconds < TestTimeMs)
                {
                    f.Rpush("foo", bytes).Exec();
                    count++;
                }
                sw.Stop();

                Assert.Fail(String.Format("{0} Push/Sec", count * 1000.0 / sw.ElapsedMilliseconds));

            }
        }

        [TearDown]
        public void TearDown()
        {
            this._redisProc.Kill();
            this._redisProc.WaitForExit();
        }
    }
}

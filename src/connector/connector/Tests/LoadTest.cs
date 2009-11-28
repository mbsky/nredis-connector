namespace Connector.Tests
{
    using System;
    using System.Diagnostics;

    using NUnit.Framework;

    [TestFixture]
    public class LoadTest
    {
        private Process _redisProc;

        const int TestTimeMs = 10000;
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
                    using (var conn = RedisConnection.Connect("localhost", 6379))
                    {
                        var f = new CommandFactory(conn);
                        f.Set(Guid.NewGuid().ToString(), "bar").Exec();
                        count++;
                    }
                }
                sw1.Stop();

                System.Diagnostics.Stopwatch sw2 = new Stopwatch();
                
                int count2 = 0;
                sw2.Start();
                using(var pool = new ConnectionPool("localhost", 6379))
                {
                    while (sw2.ElapsedMilliseconds < TestTimeMs)
                    {
                        using (var conn = pool.GetConnection())
                        {
                            var f = new CommandFactory(conn);
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
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(conn);
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
        public void GetsPerSecond()
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(conn);
                int count = 0;
                f.Set("foo", "bar").Exec();

                sw.Start();
                while (sw.ElapsedMilliseconds < TestTimeMs)
                {
                    f.Get("foo").Exec();
                    count++;
                }
                sw.Stop();

                Assert.Fail(String.Format("{0} Sets/Sec", count * 1000.0 / sw.ElapsedMilliseconds));

            }
        }
        [Test, Ignore]
        public void RPushPerSecond()
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(conn);
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

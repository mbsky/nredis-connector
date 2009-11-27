namespace Connector.Tests
{
    using System;
    using System.Diagnostics;

    using NUnit.Framework;

    [TestFixture]
    public class LoadTest
    {
        private Process _redisProc;

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
        public void SetsPerSecond()
        {
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(conn);
                int count = 0;
                sw.Start();
                while (sw.ElapsedMilliseconds < 5000)
                {
                    f.Set("foo", "bar").Exec();
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
                while (sw.ElapsedMilliseconds < 5000)
                {
                    f.Get("foo").Exec();
                    count++;
                }
                sw.Stop();

                Assert.Fail(String.Format("{0} Sets/Sec", count * 1000.0 / sw.ElapsedMilliseconds));

            }
        }

        [TearDown]
        public void TearDown()
        {
            this._redisProc.Kill();
        }
    }
}

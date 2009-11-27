namespace Connector.Tests
{
    using System;
    using System.Diagnostics;

    using NUnit.Framework;

    [TestFixture]
    public class RedisIntegrationTest
    {
        private Process _redisProc;

        public const string StatusMarker = "clients connected";

        [SetUp]
        public void Setup()
        {
            this._redisProc = Process.Start(
                new ProcessStartInfo()
                    {
                        FileName =          @"..\..\..\..\..\lib\redis\redis-server.exe",
                    });

            if (this._redisProc == null)
            {
                throw new Exception("Unable to start redis");
            }
         
        }

        [Test]
        public void SetAndGet()
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(conn);
                f.Set("foo", "bar").Exec();
                var cmd = f.Get("foo");
                cmd.Exec();
                Assert.That(cmd.Result, Is.EqualTo("bar"));
            }

        }
        
        [TearDown]
        public void TearDown()
        {
            this._redisProc.Kill();
            
        }
    }
}
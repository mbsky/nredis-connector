using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connector.Tests
{
    using System.Diagnostics;

    using NUnit.Framework;

    [TestFixture]
    public class RedisConnectionTest
    {
        private Process _redisProc;

        public const string StatusMarker = "clients connected";

        [SetUp]
        public void Setup()
        {
            _redisProc = Process.Start(
                new ProcessStartInfo()
                    {
                        FileName = @"..\..\..\..\..\lib\redis\redis-server.exe",
                        RedirectStandardOutput = true,
                        WorkingDirectory = @"..\..\..\..\..\lib\redis\", 
                        UseShellExecute = false, 
                    });

            if (this._redisProc == null)
            {
                throw new Exception("Unable to start redis");
            }
            while (!this._redisProc.StandardOutput.ReadLine().Contains(StatusMarker))
            {
            }
        }
        private string ReadLine()
        {
            for(var i = 0; i < 1; i++)
            {
                var line = this._redisProc.StandardOutput.ReadLine();
                if(!line.Contains(StatusMarker))
                {
                    return line;
                }
            }
            return null;
        }

        [Test]
        public void Connect()
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var sampleOut = this.ReadLine();
                Assert.That(sampleOut, Is.StringContaining("Accepted 127.0.0.1"));
            }

        }

        [Test]
        public void ConnectionPoolDoesnSpawnNewConnections()
        {
            using(var pool = new ConnectionPool("localhost", 6379))
            {
                using(var con = pool.GetConnection())
                {
                    var sampleOut = this.ReadLine();
                    Assert.That(sampleOut, Is.StringContaining("Accepted 127.0.0.1"));
                }
                using (var con = pool.GetConnection())
                {
                    var sampleOut = this.ReadLine();
                    Assert.That(sampleOut, Is.Null);
                }
            }
        }
        
        [TearDown]
        public void TearDown()
        {
            _redisProc.Kill();
            _redisProc.WaitForExit();
        }
    }
}

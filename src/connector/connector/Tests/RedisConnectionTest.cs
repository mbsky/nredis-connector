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
            while (true)
            {
                var line = this._redisProc.StandardOutput.ReadLine();
                if(!line.Contains(StatusMarker))
                {
                    return line;
                }
            }
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
        
        [TearDown]
        public void TearDown()
        {
            _redisProc.Kill();
            
        }
    }
}

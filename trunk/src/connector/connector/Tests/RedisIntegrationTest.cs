namespace Connector.Tests
{
    #region

    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;
using System.Collections;
    using System.Collections.Generic;

    #endregion

    [TestFixture]
    public class RedisIntegrationTest
    {
        #region Constants and Fields

        public const string StatusMarker = "clients connected";

        private Process _redisProc;

        #endregion

        #region Public Methods

        static Func<IRedisConnection, IComandExecutor>[] _executors = new Func<IRedisConnection, IComandExecutor>[] {
            (IRedisConnection q) => new NormalCommandExecutor(q), 
            (IRedisConnection q) => new PipelinedCommandExecutor(q)
        };

        [Test, TestCaseSource("_executors")]
        public void GetSetGetsPreviousValue(Func<IRedisConnection, IComandExecutor> ef)
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(ef(conn));
                f.Set("foo", Bytes("baz")).Exec();
                var cmd1 = f.GetSet("foo", Bytes("bar"));

                cmd1.Exec();

                Assert.That(Encoding.ASCII.GetString(cmd1.Result), Is.EqualTo("baz"));
            }
        }

        [Test, TestCaseSource("_executors")]
        public void GetSetGetsSetsNewValue(Func<IRedisConnection, IComandExecutor> ef)
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(ef(conn));
                f.Set("foo", Bytes("baz")).Exec();
                f.GetSet("foo", Bytes("bar")).Exec();
                var cmd1 = f.Get("foo");

                cmd1.Exec();

                Assert.That(Encoding.ASCII.GetString(cmd1.Result), Is.EqualTo("bar"));
            }
        }

        [Test, TestCaseSource("_executors")]
        public void MultiGet(Func<IRedisConnection, IComandExecutor> ef)
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(ef(conn));
                f.Set("foo1", "bar").Exec();
                f.Set("foo2", "baz").Exec();
                var cmd = f.MultiGet("foo1", "foo2");
                var strings = cmd.ExecAndReturn().Select(q => Str(q)).ToArray();
                Assert.That(strings, Is.EqualTo(new[] { "bar", "baz" }));
            }
        }

        [Test, TestCaseSource("_executors")]
        public void SetAndGet(Func<IRedisConnection, IComandExecutor> ef)
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(ef(conn));
                f.Set("foo", "bar").Exec();
                var cmd = f.Get("foo");
                cmd.Exec();
                Assert.That(Encoding.ASCII.GetString(cmd.Result), Is.EqualTo("bar"));
            }
        }
        
        [Test, TestCaseSource("_executors")]
        public void SetNotExists(Func<IRedisConnection, IComandExecutor> ef)
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(ef(conn));
                var setCmd = f.SetNotExists("foo", this.Bytes("bar"));
                setCmd.Exec();
                Assert.That(setCmd.Result, Is.EqualTo(1));
                setCmd.Exec();
                Assert.That(setCmd.Result, Is.EqualTo(0));
            }
        }

        [Test, TestCaseSource("_executors")]
        public void RPush(Func<IRedisConnection, IComandExecutor> ef)
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(ef(conn));
                var cmd = f.Rpush("foo", this.Bytes("bar"));
                cmd.Exec();
            }
        }
        
        [Test]
        public void SeveralRPushFormsASet()
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(new PipelinedCommandExecutor(conn));
                f.Rpush("foo", this.Bytes("bar1")).Exec();
                f.Rpush("foo", this.Bytes("bar2")).Exec();
                f.Rpush("foo", this.Bytes("bar3")).Exec();
                f.Rpush("foo", this.Bytes("bar4")).Exec();
                var foos = f.ListRange("foo", 0, 4).ExecAndReturn();
                Assert.That(Str(foos), Is.EqualTo(Enumerable.Range(1, 4).Select(q => "bar" + q)));
                    
            }
        } 


        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            this._redisProc =
                Process.Start(new ProcessStartInfo() { FileName = @"..\..\..\..\..\lib\redis\redis-server.exe", });

            if (this._redisProc == null)
            {
                throw new Exception("Unable to start redis");
            }
        }
        [SetUp]
        public void Setup()
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                new CommandFactory(new NormalCommandExecutor(conn)).FlushAll().Exec();
            }
        }


        [TestFixtureTearDown]
        public void TearDown()
        {
            this._redisProc.Kill();
            this._redisProc.WaitForExit();
        }

        #endregion

        #region Methods

        private byte[] Bytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        private string Str(byte[] str)
        {
            return Encoding.ASCII.GetString(str);
        }
        private string[] Str(IEnumerable<byte[]> str)
        {
            return str.Select(q => Str(q)).ToArray();
        }

        #endregion
    }
}
namespace Connector.Tests
{
    #region

    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;

    #endregion

    [TestFixture]
    public class RedisIntegrationTest
    {
        #region Constants and Fields

        public const string StatusMarker = "clients connected";

        private Process _redisProc;

        #endregion

        #region Public Methods

        [Test]
        public void GetSetGetsPreviousValue()
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(conn);
                f.Set("foo", Bytes("baz")).Exec();
                var cmd1 = f.GetSet("foo", Bytes("bar"));

                cmd1.Exec();

                Assert.That(Encoding.ASCII.GetString(cmd1.Result), Is.EqualTo("baz"));
            }
        }

        [Test]
        public void GetSetGetsSetsNewValue()
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(conn);
                f.Set("foo", Bytes("baz")).Exec();
                f.GetSet("foo", Bytes("bar")).Exec();
                var cmd1 = f.Get("foo");

                cmd1.Exec();

                Assert.That(Encoding.ASCII.GetString(cmd1.Result), Is.EqualTo("bar"));
            }
        }

        [Test]
        public void MultiGet()
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(conn);
                f.Set("foo1", "bar").Exec();
                f.Set("foo2", "baz").Exec();
                var cmd = f.MultiGet("foo1", "foo2");
                var strings = cmd.ExecAndReturn().Select(q => Str(q)).ToArray();
                Assert.That(strings, Is.EqualTo(new[] { "bar", "baz" }));
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
                Assert.That(Encoding.ASCII.GetString(cmd.Result), Is.EqualTo("bar"));
            }
        }
        [Test]
        public void SetNotExists()
        {
            using (var conn = RedisConnection.Connect("localhost", 6379))
            {
                var f = new CommandFactory(conn);
                var setCmd = f.SetNotExists("foo", this.Bytes("bar"));
                setCmd.Exec();
                Assert.That(setCmd.Result, Is.EqualTo(1));
                setCmd.Exec();
                Assert.That(setCmd.Result, Is.EqualTo(0));
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
                new CommandFactory(conn).FlushAll().Exec();
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

        #endregion
    }
}
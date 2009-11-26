namespace Connector.Tests
{
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    [TestFixture]
    public class RedisReaderTest
    {
        [Test]
        public void ReadLine()
        {
            var reader = this.GetReader("+PONG\r\n");
            var value = reader.ReadLineInner();
            var str = this.GetString(value);
            Assert.That(str, Is.EqualTo("+PONG"));
        }
        
        [Test]
        public void ReadLineWithR()
        {
            var reader = this.GetReader("+PON\rG\r\n");
            var value = reader.ReadLineInner();
            var str = this.GetString(value);
            Assert.That(str, Is.EqualTo("+PON\rG"));
        }

        [Test]
        public void ReadBulk()
        {
            var reader = this.GetReader("$6\r\nfoobar\r\n");
            var value = GetString(reader.ReadBulk());
            Assert.That(value, Is.EqualTo("foobar"));
        }
        
        [Test]
        public void MultiBulk()
        {
            var reader = this.GetReader("*4\r\n$3\r\nfoo\r\n$3\r\nbar\r\n$5\r\nHello\r\n$5\r\nWorld\r\n");
            var value = reader.ReadMultiBulk().Select(q => this.GetString(q));
            Assert.That(value, Is.EqualTo(new [] {"foo", "bar", "Hello", "World"}));
        } 

        [Test]
        public void MultiBulkWithNil()
        {
            var reader = this.GetReader("*4\r\n$3\r\nfoo\r\n$3\r\nbar\r\n$-1\r\n$5\r\nWorld\r\n");
            var value = reader.ReadMultiBulk().Select(q => q == null ? null : this.GetString(q));
            Assert.That(value, Is.EqualTo(new [] {"foo", "bar", null, "World"}));
        }

        [Test]
        public void ReadInteger()
        {
            var reader = this.GetReader(":123\r\n");
            var value = reader.ReadInteger();
            Assert.That(value, Is.EqualTo(123));
        }
        
        [Test]
        public void ReadAnyInt()
        {
            var reader = this.GetReader(":123\r\n");
            var value = reader.ReadAny();
            Assert.That(value.First()[0], Is.EqualTo(123));
        }
        
        [Test]
        public void ReadAnyLine()
        {
            var reader = this.GetReader("+PONG\r\n");
            var value = reader.ReadAny();
            Assert.That(this.GetString(value.First()), Is.EqualTo("PONG"));
        }
        [Test]
        public void ReadAnyBulk()
        {
            var reader = this.GetReader("$6\r\nfoobar\r\n");
            var value = reader.ReadAny();
            Assert.That(this.GetString(value.First()), Is.EqualTo("foobar"));
        }

        private string GetString(byte[] value)
        {
            return System.Text.Encoding.ASCII.GetString(value);
        }

        private TestableRedisReader GetReader(string chars)
        {
            var ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(chars));
            return new TestableRedisReader(new BinaryReader(ms));
        }

        internal class TestableRedisReader : RedisReader
        {
            public TestableRedisReader(BinaryReader redisStream)
                : base(redisStream)
            {
            }

            public new byte[] ReadLineInner()
            {
                return base.ReadLineInner();
            }
        }
    }
}
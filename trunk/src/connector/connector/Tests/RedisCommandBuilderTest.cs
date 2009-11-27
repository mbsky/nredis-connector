namespace Connector.Tests
{
    using System.IO;
    using System.Text;

    using NUnit.Framework;

    [TestFixture]
    public class RedisInlineCommandBuilderTest
    {
        [Test]
        public void SetCommandBuilding()
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("SET");
            builder.AddInlineArgument("foo");
            builder.SetData("data");
            var str = GetString(builder);
            Assert.That(str, Is.EqualTo("SET foo 4\r\ndata\r\n"));
        } 
        
        [Test]
        public void GetCommandBuilding()
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("GET");
            builder.AddInlineArgument("foo");
            var str = GetString(builder);
            Assert.That(str, Is.EqualTo("GET foo\r\n"));
        }

        private static string GetString(IRedisCommandBuilder builder)
        {
            var ms = new MemoryStream();
            builder.FlushCommandTo(new BinaryWriter(ms));
            return Encoding.ASCII.GetString(ms.ToArray());
        }
    }

    [TestFixture]
    public class RedisCommandBuilderTest
    {
        [Test]
        public void BuildBulkCommandTwoStringArgs()
        {
            var builder = new RedisCommandBuilder();
            
            builder.SetCommand("set");
            builder.AddArgument("foo");
            builder.AddArgument("barbaz");

            string str = GetString(builder);

            Assert.That(str, Is.EqualTo("*3\r\n$3\r\nset\r\n$3\r\nfoo\r\n$6\r\nbarbaz\r\n"));
            
        }
        
        [Test]
        public void BuildBulkCommandTwoBinaryArgs()
        {
            var builder = new RedisCommandBuilder();
            
            builder.SetCommand("set");
            builder.AddArgument(Encoding.ASCII.GetBytes("foo"));
            builder.AddArgument(Encoding.ASCII.GetBytes("barbaz"));

            string str = GetString(builder);

            Assert.That(str, Is.EqualTo("*3\r\n$3\r\nset\r\n$3\r\nfoo\r\n$6\r\nbarbaz\r\n"));
            
        }

        private static string GetString(IRedisCommandBuilder builder)
        {
            var ms = new MemoryStream();
            builder.FlushCommandTo(new BinaryWriter(ms));
            return Encoding.ASCII.GetString(ms.ToArray());
        }
    }
}
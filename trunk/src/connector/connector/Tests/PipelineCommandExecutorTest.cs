using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Connector.Tests
{
    public class ChessTest
    {
        public static bool Run()
        {
            try
            {
                new PipelineCommandExecutorTest().TwoSimultaneiousWritesReadsInSameOrder();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
    [TestFixture]
    public class PipelineCommandExecutorTest
    {
        #region Nested type: LoopbackConnection

        private class LoopbackConnection : IRedisConnection
        {
            private BinaryReader _reader;
            private BinaryWriter _writer;

            public LoopbackConnection()
            {
                var stream = new LoopbackStream();
                _writer = new BinaryWriter(stream);
                _reader = new BinaryReader(stream);
            }

            #region IRedisConnection Members

            public void Dispose()
            {
                
            }

            public BinaryWriter Writer
            {
                get { return _writer; }
            }

            public BinaryReader Reader
            {
                get { return _reader; }
            }

            public void Close()
            {
            }

            #endregion
        }

        #endregion

        #region Nested type: LoopbackStream

        private class LoopbackStream : Stream
        {
            private readonly Queue<byte> _data = new Queue<byte>();

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override long Length
            {
                get { throw new NotImplementedException(); }
            }

            public override long Position
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    lock (_data)
                    {
                        buffer[i + offset] = _data.Dequeue();
                    }
                }
                return count;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    lock (_data)
                    {
                        _data.Enqueue(buffer[i + offset]);
                    }
                }
            }
        }

        #endregion
        
        [Test]
        public void TwoSimultaneiousWritesReadsInSameOrder()
        {
            var b1 = new RedisInlineCommandBuilder();
            var b2 = new RedisInlineCommandBuilder();
            b1.SetCommand(":1");
            b2.SetCommand(":2");

            var executor = new PipelinedCommandExecutor(new LoopbackConnection());

            int result = 0;

            var t1 = new Thread(() => result = BitConverter.ToInt32(executor.ExecuteCommand(b1).First(), 0));
            var t2 = new Thread(() => executor.ExecuteCommand(b2));

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            Assert.That(result, Is.EqualTo(1));
        }

    }
}
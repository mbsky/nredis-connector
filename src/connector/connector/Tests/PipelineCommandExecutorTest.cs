using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            private LoopbackStream _stream;

            public LoopbackConnection()
            {
                _stream = new LoopbackStream();
                _writer = new BinaryWriter(_stream);
                _reader = new BinaryReader(_stream);
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

            public byte[] GetBuffer()
            {
                return _stream.GetBuffer();
            }

            public void WriteResponce()
            {
                _stream.WriteResponce();
            }

            #endregion
        }

        #endregion

        #region Nested type: LoopbackStream

        private class LoopbackStream : Stream
        {
            private readonly Queue<byte> _data = new Queue<byte>();
            private readonly ManualResetEvent _evt = new ManualResetEvent(false);

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
                _evt.WaitOne();

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
            public void WriteResponce()
            {
                _evt.Set();
            }
            public byte[] GetBuffer()
            {
                return _data.ToArray();
            }
        
        }

        #endregion

        [Test, Timeout(5000)]
        public void TwoSimultaneiousWritesReadsInSameOrder()
        {
            var b1 = new RedisInlineCommandBuilder();
            var b2 = new RedisInlineCommandBuilder();
            b1.SetCommand(":1");
            b2.SetCommand(":2");

            var loopbackConnection = new LoopbackConnection();
            loopbackConnection.WriteResponce();
            var executor = new PipelinedCommandExecutor(loopbackConnection);

            int result = 0;

            var t1 = new Thread(() => result = BitConverter.ToInt32(executor.ExecuteCommand(b1).First(), 0));
            var t2 = new Thread(() => executor.ExecuteCommand(b2));

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            executor.Dispose();

            Assert.That(result, Is.EqualTo(1));
        }
        
        [Test, Timeout(1000)]
        public void IfResponseIsBlockedThenNextCommandWillWrite()
        {
            var b1 = new RedisInlineCommandBuilder();
            var b2 = new RedisInlineCommandBuilder();
            b1.SetCommand(":1");
            b2.SetCommand(":2");

            var loopbackConnection = new LoopbackConnection();
            var executor = new PipelinedCommandExecutor(loopbackConnection);

            var t1 = new Thread(() => executor.ExecuteCommand(b1));
            var t2 = new Thread(() => executor.ExecuteCommand(b2));

            t1.Start();
            t2.Start();

            Thread.Sleep(100);
            var str = Encoding.ASCII.GetString(loopbackConnection.GetBuffer());
            loopbackConnection.WriteResponce();

            t1.Join();
            t2.Join();

            executor.Dispose();

            Assert.That(str, Is.EqualTo(":1\r\n:2\r\n"));
        }

    }
}
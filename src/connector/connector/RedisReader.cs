namespace Connector
{
    #region

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    #endregion

    public class RedisReader
    {
        #region Constants and Fields

        private readonly BinaryReader _redisStream;

        #endregion

        #region Constructors and Destructors

        public RedisReader(BinaryReader redisStream)
        {
            this._redisStream = redisStream;
        }

        #endregion

        #region Public Methods

        public IEnumerable<byte[]> ReadAny()
        {
            int firstByte = _redisStream.PeekChar();
            const int ErrorMessage = 0x2d;
            const int SingleLineReply = 0x2b;
            const int BulkData = 0x24;
            const int MultiBulk = 0x2a;
            const int IntegerReply = 0x3a;

            switch (firstByte)
            {
                case SingleLineReply:
                    return new List<byte[]> { this.ReadLine() };
                case BulkData:
                    return new List<byte[]> { this.ReadBulk() };
                case MultiBulk:
                    return this.ReadMultiBulk();
                case IntegerReply:
                    return new List<byte[]> { BitConverter.GetBytes(this.ReadInteger()) };
                default:
                    throw new Exception(Encoding.ASCII.GetString(this.ReadLineInner()));
            }
        }

        public byte[] ReadBulk()
        {
            var bulkLength = this.ReadInteger();
            if (bulkLength == -1)
            {
                return null;
            }

            var buf = new byte[bulkLength];
            _redisStream.Read(buf, 0, bulkLength);
            _redisStream.ReadByte();
            _redisStream.ReadByte();
            return buf;
        }

        public int ReadInteger()
        {
            var str = Encoding.ASCII.GetString(this.ReadLineInner());
            var bulkLength = int.Parse(str.Substring(1));
            return bulkLength;
        }

        public byte[] ReadLine()
        {
            _redisStream.ReadByte();
            return this.ReadLineInner();
        }

        protected byte[] ReadLineInner()
        {
            const int ReadBufSize = 200;
            var data = new List<byte[]>();
            byte[] buf = new byte[ReadBufSize];
            int bufIndex = 0;
            int totalCount = 0;
            while (true)
            {
                var val = _redisStream.Read();
                if (val == -1)
                {
                    break;
                }

                if (val == 0x0d)
                {
                    val = _redisStream.ReadByte();
                    if (val == 0x0a)
                    {
                        break;
                    }

                    buf[bufIndex++] = 0x0d;
                    totalCount++;
                }

                buf[bufIndex++] = (byte)val;
                totalCount++;
                if (bufIndex == ReadBufSize)
                {
                    data.Add(buf);
                    buf = new byte[ReadBufSize];
                    bufIndex = 0;
                }
            }

            data.Add(buf);
            return data.SelectMany(q => q).Take(totalCount).ToArray();
        }

        public IEnumerable<byte[]> ReadMultiBulk()
        {
            var bulkLength = this.ReadInteger();
            if (bulkLength == -1)
            {
                return null;
            }

            var list = new List<byte[]>(bulkLength);
            for (var i = 0; i < bulkLength; i++)
            {
                list.Add(this.ReadBulk());
            }
            return list;
        }

        #endregion
    }
}
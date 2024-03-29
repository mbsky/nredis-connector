﻿namespace Connector
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
        const int ErrorMessage = 0x2d;
        const int SingleLineReply = 0x2b;
        const int BulkData = 0x24;
        const int MultiBulk = 0x2a;
        const int IntegerReply = 0x3a;

        private readonly BinaryReader _redisStream;

        public RedisReader(BinaryReader redisStream)
        {
            this._redisStream = redisStream;
        }

        private void ReadFirstByteAndCheckForError(byte expected)
        {
            var firstByte = _redisStream.ReadByte();
            if (firstByte == ErrorMessage || firstByte != expected)
            {
                throw new RedisException(Encoding.ASCII.GetString(this.ReadLineInner()));
            }
        }

        public bool IsError()
        {
            return _redisStream.PeekChar() == ErrorMessage;
        }

        public IEnumerable<byte[]> ReadAny()
        {
            int firstByte = _redisStream.ReadByte();
            
            switch (firstByte)
            {
                case SingleLineReply:
                    return new List<byte[]> { this.ReadLineInner() };
                case BulkData:
                    return new List<byte[]> { this.ReadBulkInner() };
                case MultiBulk:
                    return this.ReadMultiBulkInner();
                case IntegerReply:
                    return new List<byte[]> { BitConverter.GetBytes(this.ReadIntegerInner()) };
                default:
                    throw new RedisException(Encoding.ASCII.GetString(this.ReadLineInner()));
            }
        }

        public byte[] ReadBulk()
        {
            ReadFirstByteAndCheckForError(BulkData);
            return this.ReadBulkInner();
        }

        private byte[] ReadBulkInner()
        {
            var bulkLength = this.ReadIntegerInner();
            if (bulkLength == -1)
            {
                return null;
            }

            var buf = new byte[bulkLength];
            this._redisStream.Read(buf, 0, bulkLength);
            this._redisStream.ReadByte();
            this._redisStream.ReadByte();
            return buf;
        }

        public int ReadInteger()
        {
            ReadFirstByteAndCheckForError(IntegerReply);
            return ReadIntegerInner();
        }
        private int ReadIntegerInner()
        {
            var str = Encoding.ASCII.GetString(this.ReadLineInner());
            var bulkLength = int.Parse(str);
            return bulkLength;
        }

        public string ReadLine()
        {
            ReadFirstByteAndCheckForError(SingleLineReply);
            return Encoding.ASCII.GetString( this.ReadLineInner());
        }

        protected byte[] ReadLineInner()
        {
            const int readBufSize = 200;
            var data = new List<byte[]>();
            byte[] buf = new byte[readBufSize];
            int bufIndex = 0;
            int totalCount = 0;
            while (true)
            {
                var val = _redisStream.ReadByte();
                
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
                if (bufIndex == readBufSize)
                {
                    data.Add(buf);
                    buf = new byte[readBufSize];
                    bufIndex = 0;
                }
            }

            data.Add(buf);
            return data.SelectMany(q => q).Take(totalCount).ToArray();
        }

        public IEnumerable<byte[]> ReadMultiBulk()
        {
            ReadFirstByteAndCheckForError(MultiBulk);
            return this.ReadMultiBulkInner();
        }

        private IEnumerable<byte[]> ReadMultiBulkInner()
        {
            var bulkLength = this.ReadIntegerInner();
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
    }

    public class RedisException : Exception
    {
        public RedisException(string message)
            : base(message)
        {
        }
    }
}
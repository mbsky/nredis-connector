namespace Connector
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public interface IRedisCommandBuilder
    {
        void SetCommand(string command);

        void AddInlineArgument(string arg);

        void SetData(byte[] data);

        void SetData(string data);

        void FlushCommandTo(BinaryWriter redisStream);
    }

    public class RedisInlineCommandBuilder : IRedisCommandBuilder
    {
        
        private static readonly byte[] EndLine = new byte[] { 0x0d, 0x0a };
        private static readonly byte Space = 0x20 ;

        private byte[] _command;

        private List<byte[]> _inline = new List<byte[]>();
        private byte[] _data = null;

        public void SetCommand(string command)
        {
            _command = Encoding.ASCII.GetBytes(command);
        }

        public void AddInlineArgument(string arg)
        {
            _inline.Add(Encoding.ASCII.GetBytes(arg));
        }

        public void SetData(byte[] data)
        {
            _data = data;
        }

        public void SetData(string data)
        {
            _data = Encoding.ASCII.GetBytes(data);
        }

        public void FlushCommandTo(BinaryWriter redisStream)
        {
            redisStream.Write(_command);
            
            foreach(var inline in _inline)
            {
                redisStream.Write(Space);
                redisStream.Write(inline);
            }
            if(_data != null)
            {
                redisStream.Write(Space);
                redisStream.Write(Encoding.ASCII.GetBytes(_data.Length.ToString()));
                redisStream.Write(EndLine);
                redisStream.Write(_data);
            }
            redisStream.Write(EndLine);
        }

    }

    public class RedisCommandBuilder : IRedisCommandBuilder
    {
        const int BulkData = 0x24;
        const int MultiBulk = 0x2a;

        private static readonly byte[] EndLine = new byte[] { 0x0d, 0x0a };

        private string _command;

        private List<byte[]> _arguments = new List<byte[]>();

        public void SetCommand(string command)
        {
            _command = command;
        }

        public void AddInlineArgument(string arg)
        {
            AddArgument(arg);
        }

        public void SetData(byte[] data)
        {
            AddArgument(data);
        }

        public void SetData(string data)
        {
            AddArgument(data);
        }

        public void AddArgument(string arg)
        {
            AddArgument(Encoding.ASCII.GetBytes(arg));
        }

        public void AddArgument(byte[] arg)
        {
            _arguments.Add(arg);
        }

        public void FlushCommandTo(BinaryWriter redisStream)
        {
            var bulkSize = _arguments.Count + 1;
            WriteInt(redisStream, MultiBulk, bulkSize);
            this.WriteBulk(redisStream, Encoding.ASCII.GetBytes(_command));
            foreach(var arg in _arguments)
            {
                this.WriteBulk(redisStream, arg);
            }
            _command = null;
            _arguments = new List<byte[]>();
        }

        private void WriteInt(BinaryWriter writer, byte prefix, int value)
        {
            writer.Write(prefix);
            writer.Write(Encoding.ASCII.GetBytes(value.ToString()));
            writer.Write(EndLine);
        }
        private void WriteBulk(BinaryWriter writer, byte[] bytes)
        {
            WriteInt(writer, BulkData, bytes.Length);
            writer.Write(bytes);
            writer.Write(EndLine);
        }

        public void AddInline(string foo)
        {
            throw new NotImplementedException();
        }
    }
}
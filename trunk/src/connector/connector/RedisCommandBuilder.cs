namespace Connector
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class RedisCommandBuilder
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
    }
}
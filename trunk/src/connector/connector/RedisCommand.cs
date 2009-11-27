using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connector
{
    using System.IO;

    public class RedisCommand
    {
        private readonly RedisConnection _connection;

        private readonly IRedisCommandBuilder _builder;

        public RedisCommand(RedisConnection connection, IRedisCommandBuilder builder)
        {
            _connection = connection;
            _builder = builder;
        }

        public void Exec()
        {
            var reader = new RedisReader(_connection.Reader);
            _builder.FlushCommandTo(_connection.Writer);

            if (reader.IsError())
            {
                throw new RedisException(reader.ReadLine());
            }
            ReadResult(reader);
        }

        protected virtual void ReadResult(RedisReader reader)
        {
            
        }
    }
    public class RedisCommandWithInt : RedisCommand
    {
        public RedisCommandWithInt(RedisConnection connection, RedisCommandBuilder builder)
            : base(connection, builder)
        {
        }
        public int Result { get; private set; }

        protected override void ReadResult(RedisReader reader)
        {
            Result = reader.ReadInteger();
        }
    }

    public class RedisCommandWithString : RedisCommand
    {
        public RedisCommandWithString(RedisConnection connection, RedisCommandBuilder builder)
            : base(connection, builder)
        {
        }

        public string Result { get; private set; }

        protected override void ReadResult(RedisReader reader)
        {
            Result = reader.ReadLine();
        }
    }
    public class RedisCommandWithBytes : RedisCommand
    {
        public RedisCommandWithBytes(RedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }

        public byte[] Result { get; private set; }

        protected override void ReadResult(RedisReader reader)
        {
            Result = reader.ReadAny().First();
        }
    }
}

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
            reader.ReadLine();
        }
    }

    public class RedisQuitCommand : RedisCommand
    {
        public RedisQuitCommand(RedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }
        protected override void ReadResult(RedisReader reader)
        {
        }
    }

    public abstract class RedisCommandWithResult<T> : RedisCommand
    {
        protected RedisCommandWithResult(RedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }
        public T Result { get; protected set; }
        public T ExecAndReturn()
        {
            Exec();
            return Result;
        }
    }

    public class RedisCommandWithInt : RedisCommandWithResult<int>
    {
        public RedisCommandWithInt(RedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }

        protected override void ReadResult(RedisReader reader)
        {
            Result = reader.ReadInteger();
        }
    }

    public class RedisCommandWithString :  RedisCommandWithResult<string>
    {
        public RedisCommandWithString(RedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }


        protected override void ReadResult(RedisReader reader)
        {
            Result = reader.ReadLine();
        }

    }

    public class RedisCommandWithBytes : RedisCommandWithResult<byte[]>
    {
        public RedisCommandWithBytes(RedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }

        protected override void ReadResult(RedisReader reader)
        {
            Result = reader.ReadAny().First();
        }
    } 
    public class RedisCommandWithMultiBytes : RedisCommandWithResult<IEnumerable<byte[]>>
    {
        public RedisCommandWithMultiBytes(RedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }

        protected override void ReadResult(RedisReader reader)
        {
            Result = reader.ReadMultiBulk();
        }
    }
}

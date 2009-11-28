using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connector
{
    using System.IO;

    public class RedisCommand
    {
        private readonly IRedisConnection _connection;

        private readonly IRedisCommandBuilder _builder;

        public RedisCommand(IRedisConnection connection, IRedisCommandBuilder builder)
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
        public RedisQuitCommand(IRedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }
        protected override void ReadResult(RedisReader reader)
        {
        }
    }

    public abstract class RedisCommandWithResult<T> : RedisCommand
    {
        protected RedisCommandWithResult(IRedisConnection connection, IRedisCommandBuilder builder)
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
        public RedisCommandWithInt(IRedisConnection connection, IRedisCommandBuilder builder)
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
        public RedisCommandWithString(IRedisConnection connection, IRedisCommandBuilder builder)
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
        public RedisCommandWithBytes(IRedisConnection connection, IRedisCommandBuilder builder)
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
        public RedisCommandWithMultiBytes(IRedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }

        protected override void ReadResult(RedisReader reader)
        {
            Result = reader.ReadMultiBulk();
        }
    }
}

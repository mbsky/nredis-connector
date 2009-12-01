using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connector
{
    using System.IO;

    public class RedisCommand
    {
        protected readonly IRedisCommandBuilder _builder;

        protected readonly IComandExecutor _executor;

        public RedisCommand(IComandExecutor executor, IRedisCommandBuilder builder)
        {
            _executor = executor;
            _builder = builder;
        }

        public virtual void Exec()
        {
            var data = _executor.ExecuteCommand(_builder);
            ReadResult(data);
        }

        protected virtual void ReadResult(IEnumerable<byte[]> data)
        {
        }
    }

    public class RedisQuitCommand : RedisCommand
    {
        public RedisQuitCommand(IComandExecutor executor, IRedisCommandBuilder builder)
            : base(executor, builder)
        {
        }
        public override void Exec()
        {
            _executor.ExecuteCommandWithoutResult(_builder);
        }
    }

    public abstract class RedisCommandWithResult<T> : RedisCommand
    {
        protected RedisCommandWithResult(IComandExecutor executor, IRedisCommandBuilder builder)
            : base(executor, builder)
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
        public RedisCommandWithInt(IComandExecutor executor, IRedisCommandBuilder builder)
            : base(executor, builder)
        {
        }

        protected override void ReadResult(IEnumerable<byte[]> data)
        {
            Result = BitConverter.ToInt32(data.First(), 0);
        }
    }

    public class RedisCommandWithString : RedisCommandWithResult<string>
    {
        public RedisCommandWithString(IComandExecutor executor, IRedisCommandBuilder builder)
            : base(executor, builder)
        {
        }


        protected override void ReadResult(IEnumerable<byte[]> data)
        {
            Result = Encoding.ASCII.GetString(data.First());
        }

    }

    public class RedisCommandWithBytes : RedisCommandWithResult<byte[]>
    {
        public RedisCommandWithBytes(IComandExecutor executor, IRedisCommandBuilder builder)
            : base(executor, builder)
        {
        }

        protected override void ReadResult(IEnumerable<byte[]> data)
        {
            Result = data.First();
        }
    }
    public class RedisCommandWithMultiBytes : RedisCommandWithResult<IEnumerable<byte[]>>
    {
        public RedisCommandWithMultiBytes(IComandExecutor executor, IRedisCommandBuilder builder)
            : base(executor, builder)
        {
        }

        protected override void ReadResult(IEnumerable<byte[]> data)
        {
            Result = data;
        }
    }
}

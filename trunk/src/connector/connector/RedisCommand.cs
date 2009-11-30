using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connector
{
    using System.IO;
    using System.Threading;

    public interface IComandExecutor
    {
        IEnumerable<byte[]> ExecuteCommand(IRedisConnection conn, IRedisCommandBuilder builder);
    }

    public class NormalCommandExecutor : IComandExecutor
    {
        public IEnumerable<byte[]> ExecuteCommand(IRedisConnection conn, IRedisCommandBuilder builder)
        {
            var reader = new RedisReader(conn.Reader);
            builder.FlushCommandTo(conn.Writer);
            
            if (reader.IsError())
            {
                throw new RedisException(reader.ReadLine());
            }
            
            return reader.ReadAny();
        }
    }

    public class PipelinedCommandExecutor : IComandExecutor
    {
        private object _evtLock = new object();
        private object _readLock = new object();
        private Queue<AutoResetEvent> _evts = new Queue<AutoResetEvent>();

        private PipelinedCommandExecutor() { }
        private static IComandExecutor _inst = new PipelinedCommandExecutor(); 
        public static IComandExecutor Instance
        {
            get
            {
                return _inst;
            }
        }

        public IEnumerable<byte[]> ExecuteCommand(IRedisConnection conn, IRedisCommandBuilder builder)
        {
            var reader = new RedisReader(conn.Reader);
            

            var evt = new AutoResetEvent(false);
            AutoResetEvent prevEvt = null;
            lock (_evtLock)
            {
                builder.FlushCommandTo(conn.Writer);
                _evts.Enqueue(evt);
                prevEvt = _evts.Dequeue();
            }
            prevEvt.Set();
            evt.WaitOne();
            lock (_readLock)
            {
                return reader.ReadAny().ToArray();
            }
        }
    }

    public class RedisCommand
    {
        protected readonly IRedisConnection _connection;

        protected readonly IRedisCommandBuilder _builder;

        private readonly IComandExecutor _executor = PipelinedCommandExecutor.Instance;

        public RedisCommand(IRedisConnection connection, IRedisCommandBuilder builder)
        {
            _connection = connection;
            _builder = builder;
        }

        public virtual void Exec()
        {
            var data = _executor.ExecuteCommand(_connection, _builder);

            ReadResult(data);
        }

        protected virtual void ReadResult(IEnumerable<byte[]> data)
        {
        }
    }

    public class RedisQuitCommand : RedisCommand
    {
        public RedisQuitCommand(IRedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }
        public override void Exec()
        {
            _builder.FlushCommandTo(_connection.Writer);
        }

        protected override void ReadResult(IEnumerable<byte[]> data)
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

        protected override void ReadResult(IEnumerable<byte[]> data)
        {
            Result = BitConverter.ToInt32(data.First(), 0);
        }
    }

    public class RedisCommandWithString :  RedisCommandWithResult<string>
    {
        public RedisCommandWithString(IRedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }


        protected override void ReadResult(IEnumerable<byte[]> data)
        {
            Result = Encoding.ASCII.GetString(data.First());
        }

    }

    public class RedisCommandWithBytes : RedisCommandWithResult<byte[]>
    {
        public RedisCommandWithBytes(IRedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }

        protected override void ReadResult(IEnumerable<byte[]> data)
        {
            Result = data.First();
        }
    } 
    public class RedisCommandWithMultiBytes : RedisCommandWithResult<IEnumerable<byte[]>>
    {
        public RedisCommandWithMultiBytes(IRedisConnection connection, IRedisCommandBuilder builder)
            : base(connection, builder)
        {
        }

        protected override void ReadResult(IEnumerable<byte[]> data)
        {
            Result = data;
        }
    }
}

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
        IEnumerable<byte[]> ExecuteCommand(IRedisCommandBuilder builder);
        void ExecuteCommandWithoutResult(IRedisCommandBuilder builder);
    }

    public class NormalCommandExecutor : IComandExecutor
    {
        private IRedisConnection _conn;
        public NormalCommandExecutor(IRedisConnection conn)
        {
            _conn = conn;
        }

        public IEnumerable<byte[]> ExecuteCommand(IRedisCommandBuilder builder)
        {
            ExecuteCommandWithoutResult(builder);
            var reader = new RedisReader(_conn.Reader);
            if (reader.IsError())
            {
                throw new RedisException(reader.ReadLine());
            }
            return reader.ReadAny();
        }

        #region IComandExecutor Members


        public void ExecuteCommandWithoutResult(IRedisCommandBuilder builder)
        {
            builder.FlushCommandTo(_conn.Writer);
        }

        #endregion
    }

    public class PipelinedCommandExecutor : IComandExecutor
    {
        private object _evtLock = new object();
        private object _readLock = new object();
        private Queue<AutoResetEvent> _evts = new Queue<AutoResetEvent>();

        private IRedisConnection _conn;

        public PipelinedCommandExecutor(IRedisConnection conn)
        {
            _conn = conn;
        }

        public IEnumerable<byte[]> ExecuteCommand(IRedisCommandBuilder builder)
        {
            ExecuteCommandWithoutResult(builder);
            lock (_readLock)
            {
                var reader = new RedisReader(_conn.Reader);
                return reader.ReadAny();
            }
        }
        public void ExecuteCommandWithoutResult(IRedisCommandBuilder builder)
        {
            var evt = new AutoResetEvent(false);
            AutoResetEvent prevEvt = null;
            lock (_evtLock)
            {
                builder.FlushCommandTo(_conn.Writer);
                _evts.Enqueue(evt);
                prevEvt = _evts.Dequeue();
            }
            prevEvt.Set();
            evt.WaitOne();
            
        }
    }

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

    public class RedisCommandWithString :  RedisCommandWithResult<string>
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

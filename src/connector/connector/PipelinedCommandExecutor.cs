using System.Collections.Generic;
using System.Threading;

namespace Connector
{
    using System;

    public class PipelinedCommandExecutor : IComandExecutor
    {
        private object _evtLock = new object();
        private object _readLock = new object();
        private Queue<AutoResetEvent> _evts = new Queue<AutoResetEvent>();
        private AutoResetEvent _ignoredResult = new AutoResetEvent(false);

        private IRedisConnection _conn;

        public PipelinedCommandExecutor(IRedisConnection conn)
        {
            _conn = conn;
        }

        public IEnumerable<byte[]> ExecuteCommand(IRedisCommandBuilder builder)
        {
            var resultIsReady = Exec(builder);
            resultIsReady.WaitOne();
            return ReadAny();
        }

        private IEnumerable<byte[]> ReadAny()
        {
            lock (_readLock)
            {
                var reader = new RedisReader(_conn.Reader);
                return reader.ReadAny();
            }
        }

        private AutoResetEvent Exec(IRedisCommandBuilder builder)
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
            return evt;
        }

        public void ExecuteCommandWithoutResult(IRedisCommandBuilder builder)
        {
            var resultIsReady = Exec(builder);
            resultIsReady.WaitOne();
        }

        public void Dispose()
        {
            _conn.Dispose();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Connector
{
    using System;

    public class PipelinedCommandExecutor : IComandExecutor
    {
        class WaitPair
        {
            public AutoResetEvent ResultIsReady = new AutoResetEvent(false);
            public IEnumerable<byte[]> Result;

        }

        private readonly Queue<WaitPair> _evts = new Queue<WaitPair>();

        private IRedisConnection _conn;
        private readonly RedisReader _reader;

        public PipelinedCommandExecutor(IRedisConnection conn)
        {
            _conn = conn;
            _reader = new RedisReader(_conn.Reader);
        }

        public IEnumerable<byte[]> ExecuteCommand(IRedisCommandBuilder builder)
        {
            var waitPair = new WaitPair();
            lock (_evts)
            {
                builder.FlushCommandTo(_conn.Writer);
                _evts.Enqueue(waitPair);
                var prevPair = _evts.Dequeue();
                prevPair.Result = _reader.ReadAny();
                prevPair.ResultIsReady.Set();
            }
            waitPair.ResultIsReady.WaitOne();
            return waitPair.Result;
        }

        public void ExecuteCommandWithoutResult(IRedisCommandBuilder builder)
        {
            lock (_evts)
            {
                builder.FlushCommandTo(_conn.Writer);
                if (_evts.Any())
                {
                    var prevPair = _evts.Dequeue();
                    prevPair.Result = _reader.ReadAny();
                    prevPair.ResultIsReady.Set();
                }
            }
        }

        public void Dispose()
        {
            if (_conn != null)
            {
                _conn.Dispose();
                _conn = null;
            }
        }
    }
}
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
        private readonly Thread _readThread;
        private bool _stopReading = false;
        private object _writeLock = new object();
        public PipelinedCommandExecutor(IRedisConnection conn)
        {
            _conn = conn;
            _reader = new RedisReader(_conn.Reader);
            _readThread = new Thread(ReadProcedure);
            _readThread.Start();
        }

        private void ReadProcedure()
        {
            while(!_stopReading)
            {
                WaitPair evt = null;
                lock(_evts)
                {
                    if (_evts.Any())
                    {
                        evt = _evts.Dequeue();
                    }
                }
                if(evt != null)
                {
                    evt.Result = _reader.ReadAny();
                    evt.ResultIsReady.Set();
                }
                lock(_evts)
                {
                    if (!_evts.Any())
                    {
                        Monitor.Wait(_evts);
                    }
                }

            }
        }

        public IEnumerable<byte[]> ExecuteCommand(IRedisCommandBuilder builder)
        {
            var waitPair = new WaitPair();
            lock(_writeLock)
            {
                builder.FlushCommandTo(_conn.Writer);
                lock (_evts)
                {
                    _evts.Enqueue(waitPair);
                    Monitor.Pulse(_evts);
                }
            }
            waitPair.ResultIsReady.WaitOne();
            return waitPair.Result;
        }

        public void ExecuteCommandWithoutResult(IRedisCommandBuilder builder)
        {
            lock (_writeLock)
            {
                builder.FlushCommandTo(_conn.Writer);
            }
        }

        public void Dispose()
        {
            if (_conn != null)
            {
                _stopReading = true;
                lock (_evts)
                {
                    Monitor.Pulse(_evts);
                }
                _readThread.Join();
                _conn.Dispose();
                _conn = null;
            }
        }
    }
}
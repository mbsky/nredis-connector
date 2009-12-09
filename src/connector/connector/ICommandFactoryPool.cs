using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connector
{
    public interface ICommandFactoryPool : IDisposable
    {
        CommandFactory Get();
    }

    public class NormalCommandFactoryPool : ICommandFactoryPool
    {
        private readonly ConnectionPool _pool;

        public NormalCommandFactoryPool(string host, int port)
        {
            _pool = new ConnectionPool(host, port);
        }

        public CommandFactory Get()
        {
            return new CommandFactory(new NormalCommandExecutor(_pool.GetConnection()));
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
    public class PipelinedCommandFactoryPool : ICommandFactoryPool
    {
        private readonly ConnectionPool _pool;

        private readonly PipelinedCommandExecutor[] _pipelinedExecutor;

        private int _roundRobinCounter = 0;

        public PipelinedCommandFactoryPool(string host, int port) : this(host, port, 1)
        {
        }

        public PipelinedCommandFactoryPool(string host, int port, int executorsCount)
        {
            _pool = new ConnectionPool(host, port);
            _pipelinedExecutor =
                Enumerable.Range(0, executorsCount).Select(q => new PipelinedCommandExecutor(_pool.GetConnection())).
                    ToArray();
        }

        public CommandFactory Get()
        {
            lock (this)
            {
                var executor = _pipelinedExecutor[_roundRobinCounter++];
                if(_roundRobinCounter >= _pipelinedExecutor.Length)
                {
                    _roundRobinCounter = 0;
                }
                return new CommandFactory(executor);
            }
        }

        public void Dispose()
        {
            foreach(var exec in _pipelinedExecutor)
            {
                exec.Dispose();
            }
            _pool.Dispose();
        }
    }
}

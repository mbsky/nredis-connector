using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connector
{
    interface ICommandFactoryPool : IDisposable
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
        private readonly RedisConnection _conn;

        private readonly PipelinedCommandExecutor _pipelinedExecutor;

        public PipelinedCommandFactoryPool(string host, int port)
        {
            _conn = RedisConnection.Connect(host, port);
            _pipelinedExecutor = new PipelinedCommandExecutor(_conn);
        }

        public CommandFactory Get()
        {
            return new CommandFactory(_pipelinedExecutor);
        }

        public void Dispose()
        {
            _conn.Dispose();
            _pipelinedExecutor.Dispose();
        }
    }
}

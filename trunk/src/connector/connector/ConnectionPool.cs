using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connector
{
    class ConnectionPool : IDisposable
    {
        private string _host;
        private int _port;

        private Stack<RedisConnection> _availableConnections = new Stack<RedisConnection>();
        private Stack<RedisConnection> _requestedConnections = new Stack<RedisConnection>();

        public ConnectionPool(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public IRedisConnection GetConnection()
        {
            RedisConnection conn = null;
            lock (this._availableConnections)
            {
                if (this._availableConnections.Any())
                {
                    conn = this._availableConnections.Pop();
                }
                else
                {
                    conn = RedisConnection.Connect(_host, _port);
                }
                _requestedConnections.Push(conn);
            }
            return new PooledRedisConnection(conn, ReturnConnectionCallback);
            
        }

        private void ReturnConnectionCallback(RedisConnection connection)
        {
            this._availableConnections.Push(connection);
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (var conn in this._availableConnections.Union(_requestedConnections))
            {
                conn.Dispose();
                GC.SuppressFinalize(conn);
            }
        }

        #endregion
    }

    class PooledRedisConnection : IRedisConnection
    {
        Action<RedisConnection> _callback;
        RedisConnection _connection;
        private bool _pooled = false;

        public PooledRedisConnection(RedisConnection connection, Action<RedisConnection> callback)
            : base()
        {
            _connection = connection;
            _callback = callback;
        }

        private void ReturnToPool()
        {
            if (!_pooled)
            {
                _callback(this._connection);
                _pooled = true;
            }
        }

        public void Close()
        {
            ReturnToPool();
        }

        public void Dispose()
        {
            ReturnToPool();
        }


        #region IRedisConnection Members

        public System.IO.BinaryWriter Writer
        {
            get { return _connection.Writer; }
        }

        public System.IO.BinaryReader Reader
        {
            get { return _connection.Reader; }
        }

        #endregion
    }
}

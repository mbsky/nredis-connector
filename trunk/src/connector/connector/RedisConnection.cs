using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connector
{
    using System.IO;
    using System.Net.Sockets;

    
    public class RedisConnection : IDisposable
    {
        private NetworkStream _stream;

        private Socket _socket;

        private BinaryWriter _writer;

        private BinaryReader _reader;

        protected RedisConnection()
        {
        }

        public static RedisConnection Connect(string host, int port)
        {
            var conn = new RedisConnection();
            conn.ConnectInt(host, port);
            return conn;
        }

        private void ConnectInt(string host, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(host, port);
            _stream = new NetworkStream(_socket);
            _reader = new BinaryReader(this._stream);
            _writer = new BinaryWriter(_stream);
        }

        public virtual BinaryWriter Writer
        {
            get
            {
                return this._writer;
            }
        }

        public virtual BinaryReader Reader
        {
            get
            {
                return this._reader;
            }
        }

        public void Close()
        {
            var f = new CommandFactory(this);
            f.Quit().Exec();
        }

        public void Dispose()
        {

            if (_stream != null)
            {
                Close();
                _stream.Close();
            }
            if (_socket != null)
            {
                _socket.Close();
            }
        }
    }
}


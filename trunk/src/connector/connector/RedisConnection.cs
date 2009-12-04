using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connector
{
    using System.IO;
    using System.Net.Sockets;


    public interface IRedisConnection : IDisposable
    {
        BinaryWriter Writer {get;}
        BinaryReader Reader {get;}
        void Close();
    }

    public class RedisConnection : IRedisConnection
    {
        private NetworkStream _stream;

        private Socket _socket;

        private BinaryWriter _writer;

        private BinaryReader _reader;

        private const int BufferSize = 1024;

        protected RedisConnection()
        {
        }

        public static RedisConnection Connect(string host, int port)
        {
            var conn = new RedisConnection();
            conn.ConnectInt(host, port);
            return conn;
        }

        protected void ConnectInt(string host, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(host, port);
            _stream = new NetworkStream(_socket);
            _reader = new BinaryReader(this._stream);
            var bufferedStream = new BufferedStream(this._stream, BufferSize);
            _writer = new BinaryWriter(bufferedStream);
        }

        public BinaryWriter Writer
        {
            get
            {
                return this._writer;
            }
        }

        public BinaryReader Reader
        {
            get
            {
                return this._reader;
            }
        }

        public void Close()
        {
            var f = new CommandFactory(new NormalCommandExecutor(this));
            f.Quit().Exec();
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                Close();
                _stream.Close();
                _stream = null;
            }
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }
        }
    }
}


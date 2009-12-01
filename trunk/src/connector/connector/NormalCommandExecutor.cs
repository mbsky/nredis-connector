using System.Collections.Generic;

namespace Connector
{
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
}
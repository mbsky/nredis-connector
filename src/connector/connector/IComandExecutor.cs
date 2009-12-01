using System.Collections.Generic;

namespace Connector
{
    public interface IComandExecutor
    {
        IEnumerable<byte[]> ExecuteCommand(IRedisCommandBuilder builder);
        void ExecuteCommandWithoutResult(IRedisCommandBuilder builder);
    }
}
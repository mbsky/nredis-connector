using System.Collections.Generic;

namespace Connector
{
    using System;

    public interface IComandExecutor : IDisposable
    {
        IEnumerable<byte[]> ExecuteCommand(IRedisCommandBuilder builder);
        void ExecuteCommandWithoutResult(IRedisCommandBuilder builder);
    }
}
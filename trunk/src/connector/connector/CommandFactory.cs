namespace Connector
{
    using System;

    public partial class CommandFactory : IDisposable
    {
        private readonly IComandExecutor _executor;

        public CommandFactory(IComandExecutor executor)
        {
            _executor = executor;
        }

        private static RedisInlineCommandBuilder For1Args(string command, string key)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand(command);
            builder.AddInlineArgument(key);
            return builder;
        }
        
        private static RedisInlineCommandBuilder For0Args(string command)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand(command);
            return builder;
        }

        private static RedisInlineCommandBuilder For2Args(string command, string key1, string key2)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand(command);
            builder.AddInlineArgument(key1);
            builder.AddInlineArgument(key2);
            return builder;
        }

        private static RedisInlineCommandBuilder For2Args(string command, string key, int value)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand(command);
            builder.AddInlineArgument(key);
            builder.AddInlineArgument(value.ToString());
            return builder;
        }

        private static RedisInlineCommandBuilder For2Args(string command, string key, byte[] value)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand(command);
            builder.AddInlineArgument(key);
            builder.SetData(value);
            return builder;
        }

        private static RedisInlineCommandBuilder For3Args(string command, string key, string v1, byte[] data)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand(command);
            builder.AddInlineArgument(key);
            builder.AddInlineArgument(v1);
            builder.SetData(data);

            return builder;
        }
        private static RedisInlineCommandBuilder For3Args(string command, string key, int v1, byte[] data)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand(command);
            builder.AddInlineArgument(key);
            builder.AddInlineArgument(v1.ToString());
            builder.SetData(data);

            return builder;
        }
        private static RedisInlineCommandBuilder For3Args(string command, string key, int v1, int v2)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand(command);
            builder.AddInlineArgument(key);
            builder.AddInlineArgument(v1.ToString());
            builder.AddInlineArgument(v2.ToString());
            
            return builder;
        }

        public RedisCommandWithString RandomKey()
        {
            var builder = For0Args("RANDOMKEY");
            return new RedisCommandWithString(this._executor, builder);
        }
        public RedisCommandWithInt DbSize()
        {
            var builder = For0Args("DBSIZE");
            return new RedisCommandWithInt(this._executor, builder);
        }

        public RedisCommand Set(string key, string value)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("SET");
            builder.AddInlineArgument(key);
            builder.SetData(value);
            return new RedisCommand(this._executor, builder);
        }
        
        public RedisCommandWithBytes Get(string key)
        {
            var builder = For1Args("GET", key);
            return new RedisCommandWithBytes (this._executor, builder);
        }

        public RedisCommandWithMultiBytes MultiGet(params string[] keys)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("MGET");
            foreach (var key in keys)
            {
                builder.AddInlineArgument(key);
            }
            return new RedisCommandWithMultiBytes(_executor, builder);
        }

        public RedisCommand FlushAll()
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("FLUSHALL");
            return new RedisCommand(_executor, builder);
        }

        public RedisCommand Quit()
        {
            var builder = For0Args("QUIT");
            return new RedisQuitCommand(_executor, builder);
        }

        public RedisCommand Rpush(string foo, byte[] bytes)
        {
            var builder = For2Args("RPUSH", foo, bytes);
            return new RedisCommand(_executor, builder);
        }

        public void Dispose()
        {
            _executor.Dispose();
        }
    }

    
}
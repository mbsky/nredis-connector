namespace Connector
{
    using System;

    public class CommandFactory
    {
        private readonly IRedisConnection _connection;

        public CommandFactory(IRedisConnection connection)
        {
            this._connection = connection;
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

        private static RedisInlineCommandBuilder For2Args(string command, string key, byte[] value)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand(command);
            builder.AddInlineArgument(key);
            builder.SetData(value);
            return builder;
        }

        public RedisCommand Set(string key, string value)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("SET");
            builder.AddInlineArgument(key);
            builder.SetData(value);
            return new RedisCommand(this._connection, builder);
        }
        
        public RedisCommand Set(string key, byte[] value)
        {
            var builder = For2Args("SET", key, value);
            return new RedisCommand(this._connection, builder);
        }

        public RedisCommandWithBytes GetSet(string key, byte[] value)
        {
            var builder = For2Args("GETSET", key, value);
            return new RedisCommandWithBytes(this._connection, builder);
        }
        public RedisCommandWithInt SetNotExists(string key, byte[] value)
        {
            var builder = For2Args("SETNX", key, value);
            return new RedisCommandWithInt(this._connection, builder);
        }

        public RedisCommandWithBytes Get(string key)
        {
            var builder = For1Args("GET", key);
            return new RedisCommandWithBytes (this._connection, builder);
        }

        public RedisCommandWithMultiBytes MultiGet(params string[] keys)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("MGET");
            foreach (var key in keys)
            {
                builder.AddInlineArgument(key);
            }
            return new RedisCommandWithMultiBytes(_connection, builder);
        }

        public RedisCommand FlushAll()
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("FLUSHALL");
            return new RedisCommand(_connection, builder);
        }
        public RedisCommand Quit()
        {
            var builder = For0Args("QUIT");
            return new RedisQuitCommand(_connection, builder);
        }

        public RedisCommand Rpush(string foo, byte[] bytes)
        {
            var builder = For2Args("RPUSH", foo, bytes);
            return new RedisCommand(_connection, builder);
        }
    }

    
}
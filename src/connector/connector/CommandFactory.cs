namespace Connector
{
    using System;

    public class CommandFactory
    {
        private readonly RedisConnection _connection;

        public CommandFactory(RedisConnection connection)
        {
            this._connection = connection;
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
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("SET");
            builder.AddInlineArgument(key);
            builder.SetData(value);
            return new RedisCommand(this._connection, builder);
        }

        public RedisCommandWithBytes GetSet(string key, byte[] value)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("GETSET");
            builder.AddInlineArgument(key);
            builder.SetData(value);
            return new RedisCommandWithBytes(this._connection, builder);
        }

        public RedisCommandWithBytes Get(string key)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("GET");
            builder.AddInlineArgument(key);
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

        public RedisCommandWithInt SetNotExists(string key, byte[] value)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("SETNX");
            builder.AddInlineArgument(key);
            builder.SetData(value);
            return new RedisCommandWithInt(this._connection, builder);
        }
    }

    
}
namespace Connector
{
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

        public RedisCommandWithBytes Get(string key)
        {
            var builder = new RedisInlineCommandBuilder();
            builder.SetCommand("GET");
            builder.AddInlineArgument(key);
            return new RedisCommandWithBytes (this._connection, builder);
        }
    }
}
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
            var builder = new RedisCommandBuilder();
            builder.SetCommand("SET");
            builder.AddArgument(key);
            builder.AddArgument(value);
            return new RedisCommand(this._connection, builder);
        }

        public RedisCommandWithBytes Get(string key)
        {
            var builder = new RedisCommandBuilder();
            builder.SetCommand("GET");
            builder.AddArgument(key);
            return new RedisCommandWithBytes (this._connection, builder);
        }
    }
}
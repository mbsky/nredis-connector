
namespace Connector
{
    using System;

    public partial class CommandFactory
    {
        public RedisCommand Auth(string value0)
        {
            var builder = For1Args("AUTH", value0);
            return new RedisCommand(this._executor, builder);
        }
        public RedisCommandWithBytes Get(string key, byte[] value0)
        {
            var builder = For2Args("GET", key, value0);
            return new RedisCommandWithBytes(this._executor, builder);
        }
        public RedisCommand Set(string key, byte[] value0)
        {
            var builder = For2Args("SET", key, value0);
            return new RedisCommand(this._executor, builder);
        }
        public RedisCommandWithBytes GetSet(string key, byte[] value0)
        {
            var builder = For2Args("GETSET", key, value0);
            return new RedisCommandWithBytes(this._executor, builder);
        }
        public RedisCommandWithInt SetNotExists(string key, byte[] value0)
        {
            var builder = For2Args("SETNX", key, value0);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithInt Increment(string key)
        {
            var builder = For1Args("INCR", key);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithInt IncrementBy(string key, int value0)
        {
            var builder = For2Args("INCRBY", key, value0);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithInt Decrement(string key)
        {
            var builder = For1Args("DECR", key);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithInt DecrementBy(string key, int value0)
        {
            var builder = For2Args("DECRBY", key, value0);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithInt Exists(string key)
        {
            var builder = For1Args("EXISTS", key);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithInt Delete(string key)
        {
            var builder = For1Args("DEL", key);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithString Type(string key)
        {
            var builder = For1Args("TYPE", key);
            return new RedisCommandWithString(this._executor, builder);
        }
        public RedisCommandWithBytes Keys(string key)
        {
            var builder = For1Args("KEYS", key);
            return new RedisCommandWithBytes(this._executor, builder);
        }
        public RedisCommandWithInt Rename(string value0, string value1)
        {
            var builder = For2Args("RENAME", value0, value1);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithInt RenameNotExists(string value0, string value1)
        {
            var builder = For2Args("RENAMENX", value0, value1);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithInt Expire(string key, int value0)
        {
            var builder = For2Args("EXPIRE", key, value0);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithInt TimeToLive(string key)
        {
            var builder = For1Args("TTL", key);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommand ListRPush(string key, byte[] value0)
        {
            var builder = For2Args("RPUSH", key, value0);
            return new RedisCommand(this._executor, builder);
        }
        public RedisCommand ListLPush(string key, byte[] value0)
        {
            var builder = For2Args("LPUSH", key, value0);
            return new RedisCommand(this._executor, builder);
        }
        public RedisCommand ListLen(string key)
        {
            var builder = For1Args("LLEN", key);
            return new RedisCommand(this._executor, builder);
        }
        public RedisCommandWithMultiBytes ListRange(string key, int value0, int value1)
        {
            var builder = For3Args("LRANGE", key, value0, value1);
            return new RedisCommandWithMultiBytes(this._executor, builder);
        }
        public RedisCommand ListTrim(string key, int value0, int value1)
        {
            var builder = For3Args("LTRIM", key, value0, value1);
            return new RedisCommand(this._executor, builder);
        }
        public RedisCommandWithBytes ListByIndex(string key, int value0)
        {
            var builder = For2Args("LINDEX", key, value0);
            return new RedisCommandWithBytes(this._executor, builder);
        }
        public RedisCommand ListSetByIndex(string key, int value0, byte[] value1)
        {
            var builder = For3Args("LSET", key, value0, value1);
            return new RedisCommand(this._executor, builder);
        }
        public RedisCommand ListRemove(string key, int value0, byte[] value1)
        {
            var builder = For3Args("LREM", key, value0, value1);
            return new RedisCommand(this._executor, builder);
        }
        public RedisCommandWithBytes ListLPop(string key)
        {
            var builder = For1Args("LPOP", key);
            return new RedisCommandWithBytes(this._executor, builder);
        }
        public RedisCommandWithBytes ListRPop(string key)
        {
            var builder = For1Args("RPOP", key);
            return new RedisCommandWithBytes(this._executor, builder);
        }
        public RedisCommandWithBytes ListRPopLPush(string value0, string value1)
        {
            var builder = For2Args("RPOPLPUSH", value0, value1);
            return new RedisCommandWithBytes(this._executor, builder);
        }
        public RedisCommandWithInt SetAdd(string key, byte[] value0)
        {
            var builder = For2Args("SADD", key, value0);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithInt SetRemove(string key, byte[] value0)
        {
            var builder = For2Args("SREM", key, value0);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithInt SetMove(string value0, string value1, byte[] value2)
        {
            var builder = For3Args("SMOVE", value0, value1, value2);
            return new RedisCommandWithInt(this._executor, builder);
        }
        public RedisCommandWithInt SetCardinality(string key)
        {
            var builder = For1Args("SCARD", key);
            return new RedisCommandWithInt(this._executor, builder);
        }
    }
}

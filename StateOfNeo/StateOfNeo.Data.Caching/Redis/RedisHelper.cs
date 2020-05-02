using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateOfNeo.Data.Caching.Redis
{
    public partial class RedisHelper
    {
        private int DbNum { get; }
        private readonly ConnectionMultiplexer _conn;
        public string CustomKey;

        public RedisHelper(int dbNum = 0)
                : this(dbNum, null)
        {
        }

        public RedisHelper(int dbNum, string readWriteHosts)
        {
            DbNum = dbNum;
            _conn =
                string.IsNullOrWhiteSpace(readWriteHosts) ?
                RedisConnectionHelp.Instance :
                RedisConnectionHelp.GetConnectionMultiplexer(readWriteHosts);
        }

        #region Tool

        private string AddSysCustomKey(string oldKey)
        {
            var prefixKey = CustomKey ?? RedisConnectionHelp.SysCustomKey;
            return prefixKey + oldKey;
        }

        private T Do<T>(Func<IDatabase, T> func)
        {
            var database = _conn.GetDatabase(DbNum);
            return func(database);
        }

        private void Do(Action<IDatabase> action)
        {
            var database = _conn.GetDatabase(DbNum);
            action(database);
        }

        private string ConvertToJson<T>(T value)
        {
            string result = value is string ? value.ToString() : JsonConvert.SerializeObject(value);
            return result;
        }

        private RedisValue[] ConvertToJson<T>(T[] values)
        {
            var result = new List<RedisValue>();
            foreach (var item in values)
            {
                if (item is string)
                {
                    result.Add(item as string);
                }
                else
                {
                    result.Add(JsonConvert.SerializeObject(item));
                }
            }
            return result.ToArray();
        }

        private T ConvertToObj<T>(RedisValue value)
        {
            if (typeof(T).Name.Equals(typeof(string).Name))
            {
                return JsonConvert.DeserializeObject<T>($"'{value}'");
            }
            return JsonConvert.DeserializeObject<T>(value);
        }

        private List<T> ConvertToList<T>(RedisValue[] values)
        {
            List<T> result = new List<T>();
            foreach (var item in values)
            {
                var model = ConvertToObj<T>(item);
                result.Add(model);
            }
            return result;
        }

        private RedisKey[] ConvertToRedisKeys(List<string> redisKeys)
        {
            return redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();
        }

        #endregion Tool

        #region Others

        /// <summary>
        /// Create transaction
        /// </summary>
        /// <returns>The created transaction</returns>
        public ITransaction CreateTransaction()
        {
            return GetDatabase().CreateTransaction();
        }

        public IDatabase GetDatabase()
        {
            return _conn.GetDatabase(DbNum);
        }

        public IServer GetServer(string hostAndPort)
        {
            return _conn.GetServer(hostAndPort);
        }

        /// <summary>
        /// Set system custom key
        /// </summary>
        /// <param name="customKey"></param>
        public void SetSysCustomKey(string customKey)
        {
            CustomKey = customKey;
        }

        #endregion Others
    }
}
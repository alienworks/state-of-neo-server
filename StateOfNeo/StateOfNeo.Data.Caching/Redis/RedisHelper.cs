using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        #region Key

        /// <summary>
        /// Delete one key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>True if the key was removed</returns>
        public bool KeyDelete(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyDelete(key));
        }

        /// <summary>
        /// Delete multiple keys
        /// </summary>
        /// <param name="keys">redis key</param>
        /// <returns>The number of keys that were removed</returns>
        public long KeyDelete(List<string> keys)
        {
            List<string> newKeys = keys.Select(AddSysCustomKey).ToList();
            return Do(db => db.KeyDelete(ConvertToRedisKeys(newKeys)));
        }

        /// <summary>
        /// If key exists
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>Returns if key exists</returns>
        public bool KeyExists(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyExists(key));
        }

        /// <summary>
        /// Rename key
        /// </summary>
        /// <param name="key">old redis key</param>
        /// <param name="newKey">new redis key</param>
        /// <returns>True if the key was renamed, false otherwise</returns>
        public bool KeyRename(string key, string newKey)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyRename(key, newKey));
        }

        /// <summary>
        /// Set a timeout on key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns>true if the timeout was set. false if key does not exist or the timeout could not be set.</returns>
        public bool KeyExpire(string key, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            return Do(db => db.KeyExpire(key, expiry));
        }

        #endregion key
    }
}

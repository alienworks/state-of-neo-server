using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StateOfNeo.Data.Caching.Redis
{
    public partial class RedisHelper
    {
        #region Sync

        /// <summary>
        /// Set string value
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="expiration">expiration time</param>
        /// <returns></returns>
        public bool StringSet(string key, string value, TimeSpan? expiration = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringSet(key, value, expiration));
        }

        /// <summary>
        /// Set multiple keyvalues
        /// </summary>
        /// <param name="keyValues">list of KeyVlues</param>
        /// <returns>True if the keys were set, else False</returns>
        public bool StringSet(List<KeyValuePair<RedisKey, RedisValue>> keyValues)
        {
            List<KeyValuePair<RedisKey, RedisValue>> newkeyValues =
                keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(AddSysCustomKey(p.Key), p.Value)).ToList();
            return Do(db => db.StringSet(newkeyValues.ToArray()));
        }

        /// <summary>
        /// Set object value
        /// </summary>
        /// <typeparam name="T">object value</typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiration">expiration time</param>
        /// <returns></returns>
        public bool StringSet<T>(string key, T obj, TimeSpan? expiration = default(TimeSpan?))
        {
            key = AddSysCustomKey(key);
            string json = ConvertToJson(obj);
            return Do(db => db.StringSet(key, json, expiration));
        }

        /// <summary>
        /// Get string key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>string value</returns>
        public string StringGet(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringGet(key));
        }

        /// <summary>
        /// Get multiple redis values
        /// </summary>
        /// <param name="listKey">list of string keys</param>
        /// <returns>list of redis values</returns>
        public RedisValue[] StringGet(List<string> listKey)
        {
            List<string> newKeys = listKey.Select(AddSysCustomKey).ToList();
            return Do(db => db.StringGet(ConvertToRedisKeys(newKeys)));
        }

        /// <summary>
        /// Get object key
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="key">string key</param>
        /// <returns>object value</returns>
        public T StringGet<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db => ConvertToObj<T>(db.StringGet(key)));
        }

        /// <summary>
        /// Increment value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">The amount to increment by (defaults to 1 and can be negative)</param>
        /// <returns>The value of key after the increment</returns>
        public double StringIncrement(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringIncrement(key, val));
        }

        /// <summary>
        /// Decrement value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">The amount to decrement by (defaults to 1 and can be negative)</param>
        /// <returns>The value of key after the decrement</returns>
        public double StringDecrement(string key, double val = 1)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.StringDecrement(key, val));
        }

        #endregion Sync
    }
}

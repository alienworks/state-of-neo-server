using System;
using System.Collections.Generic;
using System.Linq;

namespace StateOfNeo.Data.Caching.Redis
{
    public partial class RedisHelper
    {
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
    }
}
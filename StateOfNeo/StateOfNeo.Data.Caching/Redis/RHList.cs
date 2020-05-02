using System.Collections.Generic;
using System.Linq;

namespace StateOfNeo.Data.Caching.Redis
{
    public partial class RedisHelper
    {
        #region Sync

        /// <summary>
        /// Set element by index
        /// </summary>
        /// <param name="key"></param>
        /// <param name="index"></param>
        public void ListSetByIndex<T>(string key, long index, T value)
        {
            key = AddSysCustomKey(key);
            string json = ConvertToJson(value);
            Do(db => db.ListSetByIndex(key, index, json));
        }

        /// <summary>
        /// Get list element by index
        /// </summary>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns>element</returns>
        public T ListGetByIndex<T>(string key, long index)
        {
            key = AddSysCustomKey(key);
            return Do(db => ConvertToObj<T>(db.ListGetByIndex(key, index)));
        }

        /// <summary>
        /// Remove one element of the list
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>The number of removed elements</returns>
        public long ListRemove<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.ListRemove(key, ConvertToJson(value)));
        }

        /// <summary>
        /// Get list
        /// </summary>
        /// <param name="key"></param>
        /// <returns>list</returns>
        public List<T> ListRange<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(redis =>
            {
                var values = redis.ListRange(key);
                return ConvertToList<T>(values);
            });
        }

        /// <summary>
        /// Enqueue from the right
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>The length of the list after the push operation</returns>
        public long ListRightPush<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.ListRightPush(key, ConvertToJson(value)));
        }

        /// <summary>
        /// Enqueue element from the right
        /// Fixed length list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>The result of this operation</returns>
        public bool ListRightPushLimit<T>(string key, T value, int length)
        {
            if (value == null || length <= 0) return false;
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                db.ListRightPush(key, ConvertToJson(value));
                db.ListTrim(key, 1, length);
                return true;
            });
        }

        /// <summary>
        /// Enqueue elements from the right
        /// Fixed length list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>The result of this operation</returns>
        public bool ListRightPushLimit<T>(string key, T[] values, int length)
        {
            if (values == null || length <= 0) return false;
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                db.ListRightPush(key, ConvertToJson(values));
                db.ListTrim(key, values.Count(), length - 1 + values.Count());
                return true;
            });
        }

        /// <summary>
        /// Dequeue from the right
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListRightPop<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                var value = db.ListRightPop(key);
                return ConvertToObj<T>(value);
            });
        }

        /// <summary>
        /// Enqueue one element from the left
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>The length of the list after the push operations</returns>
        public long ListLeftPush<T>(string key, T value)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.ListLeftPush(key, ConvertToJson(value)));
        }

        /// <summary>
        /// Enqueue multiple elements from the left
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>The length of the list after the push operations</returns>
        public long ListLeftPush<T>(string key, T[] values)
        {
            key = AddSysCustomKey(key);
            return Do(db => db.ListLeftPush(key, ConvertToJson(values)));
        }

        /// <summary>
        /// Enqueue element from the left
        /// Fixed length list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>The result of this operation</returns>
        public bool ListLeftPushLimit<T>(string key, T value, int length)
        {
            if (value == null || length <= 0) return false;
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                db.ListLeftPush(key, ConvertToJson(value));
                db.ListTrim(key, 0, length - 1);
                return true;
            });
        }

        /// <summary>
        /// Enqueue elements from the left
        /// Fixed length list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns>The result of this operation</returns>
        public bool ListLeftPushLimit<T>(string key, T[] values, int length)
        {
            if (values == null || length <= 0) return false;
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                db.ListLeftPush(key, ConvertToJson(values));
                db.ListTrim(key, 0, length - 1);
                return true;
            });
        }

        /// <summary>
        /// Dequeue from the left
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListLeftPop<T>(string key)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                var value = db.ListLeftPop(key);
                return ConvertToObj<T>(value);
            });
        }

        /// <summary>
        /// Trim list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns>trimmed list</returns>
        public List<T> ListTrim<T>(string key, int startIndex, int endIndex)
        {
            key = AddSysCustomKey(key);
            return Do(db =>
            {
                db.ListTrim(key, startIndex, endIndex);
                var values = db.ListRange(key);
                return ConvertToList<T>(values);
            });
        }

        //public List<T> ListTrim<T>(string key, int startIndex, int length)
        //{
        //}

        /// <summary>
        /// Get list length
        /// </summary>
        /// <param name="key"></param>
        /// <returns>list length</returns>
        public long ListLength(string key)
        {
            key = AddSysCustomKey(key);
            return Do(redis => redis.ListLength(key));
        }

        #endregion Sync
    }
}
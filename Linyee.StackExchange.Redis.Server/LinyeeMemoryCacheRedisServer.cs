using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using StackExchange.Redis;
using StackExchange.Redis.Server;

namespace Linyee.StackExchange.Redis.Server
{
    /// <summary>
    /// 服务端
    /// 增加数据库的支持
    /// </summary>
    [Author("Linyeee","2019-03-25")]
    public class LinyeeMemoryCacheRedisServer: MemoryCacheRedisServer
    {

        /// <summary>
        /// 服务端
        /// </summary>
        /// <param name="databases"></param>
        /// <param name="output"></param>
        public LinyeeMemoryCacheRedisServer(int databases, TextWriter output = null) : base(databases, output)
        {
        }

        /// <summary>
        /// 服务端
        /// </summary>
        /// <param name="output"></param>
        public LinyeeMemoryCacheRedisServer(TextWriter output = null) : this(16,output) {
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                foreach (var cache in _cache_dict.Values)
                {
                    cache.Dispose();
                }
                _cache_dict.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        protected override long Dbsize(int database)
        {
            _cache = _cache_dict[database];
            return base.Dbsize(database);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override RedisValue Get(int database, RedisKey key)
        {
            //Console.WriteLine(database+"数据库Get"  );
            _cache = _cache_dict[database];
            return base.Get(database, key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected override void Set(int database, RedisKey key, RedisValue value)
        {
            //Console.WriteLine(database + "数据库Set");
            _cache = _cache_dict[database];
            base.Set(database, key, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override bool Del(int database, RedisKey key)
        {
            _cache = _cache_dict[database];
            return base.Del(database, key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        protected override void Flushdb(int database)
        {
            _cache = _cache_dict[database];
            base.Flushdb(database);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override bool Exists(int database, RedisKey key) {
            _cache = _cache_dict[database];
            return base.Exists(database, key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        protected override IEnumerable<RedisKey> Keys(int database, RedisKey pattern)
        {
            _cache = _cache_dict[database];
            return base.Keys(database, pattern);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override bool Sadd(int database, RedisKey key, RedisValue value) {
            _cache = _cache_dict[database];
            return base.Sadd(database, key,value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override bool Sismember(int database, RedisKey key, RedisValue value) {
            _cache = _cache_dict[database];
            return base.Sismember(database, key, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override bool Srem(int database, RedisKey key, RedisValue value)
        {
            _cache = _cache_dict[database];
            return base.Srem(database, key, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override long Scard(int database, RedisKey key) {
            _cache = _cache_dict[database];
            return base.Scard(database, key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override RedisValue Spop(int database, RedisKey key)
        {
            _cache = _cache_dict[database];
            return base.Spop(database, key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override long Lpush(int database, RedisKey key, RedisValue value)
        {
            _cache = _cache_dict[database];
            return base.Lpush(database, key, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override RedisValue Lpop(int database, RedisKey key)
        {
            _cache = _cache_dict[database];
            return base.Lpop(database, key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override long Llen(int database, RedisKey key)
        {
            _cache = _cache_dict[database];
            return base.Llen(database, key);
        }


        /// <summary>
        /// ;
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="arr"></param>
        protected override void LRange(int database, RedisKey key, long start, Span<TypedRedisValue> arr)
        {
            _cache = _cache_dict[database];
            base.LRange(database, key, start, arr);
        }

    }
}

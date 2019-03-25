using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace StackExchange.Redis.Server
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class RedisServer : RespServer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsMatch(string pattern, string key)
        {
            // non-trivial wildcards not implemented yet!
            return pattern == "*" || string.Equals(pattern, key, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="databases"></param>
        /// <param name="output"></param>
        protected RedisServer(int databases = 16, TextWriter output = null) : base(output)
        {
            if (databases < 1) throw new ArgumentOutOfRangeException(nameof(databases));
            Databases = databases;
            var config = ServerConfiguration;
            config["timeout"] = "0";
            config["slave-read-only"] = "yes";
            config["databases"] = databases.ToString();
            config["slaveof"] = "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        protected override void AppendStats(StringBuilder sb)
        {
            base.AppendStats(sb);
            sb.Append("Databases: ").Append(Databases).AppendLine();
            lock (ServerSyncLock)
            {
                for (int i = 0; i < Databases; i++)
                {
                    try
                    {
                        sb.Append("Database ").Append(i).Append(": ").Append(Dbsize(i)).AppendLine(" keys");
                    }
                    catch { }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Databases { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-3)]
        protected virtual TypedRedisValue Sadd(RedisClient client, RedisRequest request)
        {
            int added = 0;
            var key = request.GetKey(1);
            for (int i = 2; i < request.Count; i++)
            {
                if (Sadd(client.Database, key, request.GetValue(i)))
                    added++;
            }
            return TypedRedisValue.Integer(added);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual bool Sadd(int database, RedisKey key, RedisValue value) => throw new NotSupportedException();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-3)]
        protected virtual TypedRedisValue Srem(RedisClient client, RedisRequest request)
        {
            int removed = 0;
            var key = request.GetKey(1);
            for (int i = 2; i < request.Count; i++)
            {
                if (Srem(client.Database, key, request.GetValue(i)))
                    removed++;
            }
            return TypedRedisValue.Integer(removed);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual bool Srem(int database, RedisKey key, RedisValue value) => throw new NotSupportedException();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2)]
        protected virtual TypedRedisValue Spop(RedisClient client, RedisRequest request)
            => TypedRedisValue.BulkString(Spop(client.Database, request.GetKey(1)));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual RedisValue Spop(int database, RedisKey key) => throw new NotSupportedException();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2)]
        protected virtual TypedRedisValue Scard(RedisClient client, RedisRequest request)
            => TypedRedisValue.Integer(Scard(client.Database, request.GetKey(1)));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual long Scard(int database, RedisKey key) => throw new NotSupportedException();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(3)]
        protected virtual TypedRedisValue Sismember(RedisClient client, RedisRequest request)
            => Sismember(client.Database, request.GetKey(1), request.GetValue(2)) ? TypedRedisValue.One : TypedRedisValue.Zero;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual bool Sismember(int database, RedisKey key, RedisValue value) => throw new NotSupportedException();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(3, "client", "setname", LockFree = true)]
        protected virtual TypedRedisValue ClientSetname(RedisClient client, RedisRequest request)
        {
            client.Name = request.GetString(2);
            return TypedRedisValue.OK;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2, "client", "getname", LockFree = true)]
        protected virtual TypedRedisValue ClientGetname(RedisClient client, RedisRequest request)
            => TypedRedisValue.BulkString(client.Name);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(3, "client", "reply", LockFree = true)]
        protected virtual TypedRedisValue ClientReply(RedisClient client, RedisRequest request)
        {
            if (request.IsString(2, "on")) client.SkipReplies = -1; // reply to nothing
            else if (request.IsString(2, "off")) client.SkipReplies = 0; // reply to everything
            else if (request.IsString(2, "skip")) client.SkipReplies = 2; // this one, and the next one
            else return TypedRedisValue.Error("ERR syntax error");
            return TypedRedisValue.OK;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-1)]
        protected virtual TypedRedisValue Cluster(RedisClient client, RedisRequest request)
            => request.CommandNotFound();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-3)]
        protected virtual TypedRedisValue Lpush(RedisClient client, RedisRequest request)
        {
            var key = request.GetKey(1);
            long length = -1;
            for (int i = 2; i < request.Count; i++)
            {
                length = Lpush(client.Database, key, request.GetValue(i));
            }
            return TypedRedisValue.Integer(length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-3)]
        protected virtual TypedRedisValue Rpush(RedisClient client, RedisRequest request)
        {
            var key = request.GetKey(1);
            long length = -1;
            for (int i = 2; i < request.Count; i++)
            {
                length = Rpush(client.Database, key, request.GetValue(i));
            }
            return TypedRedisValue.Integer(length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2)]
        protected virtual TypedRedisValue Lpop(RedisClient client, RedisRequest request)
            => TypedRedisValue.BulkString(Lpop(client.Database, request.GetKey(1)));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2)]
        protected virtual TypedRedisValue Rpop(RedisClient client, RedisRequest request)
            => TypedRedisValue.BulkString(Rpop(client.Database, request.GetKey(1)));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2)]
        protected virtual TypedRedisValue Llen(RedisClient client, RedisRequest request)
            => TypedRedisValue.Integer(Llen(client.Database, request.GetKey(1)));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual long Lpush(int database, RedisKey key, RedisValue value) => throw new NotSupportedException();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual long Rpush(int database, RedisKey key, RedisValue value) => throw new NotSupportedException();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual long Llen(int database, RedisKey key) => throw new NotSupportedException();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual RedisValue Rpop(int database, RedisKey key) => throw new NotSupportedException();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual RedisValue Lpop(int database, RedisKey key) => throw new NotSupportedException();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(4)]
        protected virtual TypedRedisValue LRange(RedisClient client, RedisRequest request)
        {
            var key = request.GetKey(1);
            long start = request.GetInt64(2), stop = request.GetInt64(3);

            var len = Llen(client.Database, key);
            if (len == 0) return TypedRedisValue.EmptyArray;

            if (start < 0) start = len + start;
            if (stop < 0) stop = len + stop;

            if (stop < 0 || start >= len || stop < start) return TypedRedisValue.EmptyArray;

            if (start < 0) start = 0;
            else if (start >= len) start = len - 1;

            if (stop < 0) stop = 0;
            else if (stop >= len) stop = len - 1;

            var arr = TypedRedisValue.Rent(checked((int)((stop - start) + 1)), out var span);
            LRange(client.Database, key, start, span);
            return arr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="arr"></param>
        protected virtual void LRange(int database, RedisKey key, long start, Span<TypedRedisValue> arr) => throw new NotSupportedException();
        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnUpdateServerConfiguration() { }
        /// <summary>
        /// 
        /// </summary>
        protected RedisConfig ServerConfiguration { get; } = RedisConfig.Create();
        /// <summary>
        /// 
        /// </summary>
        protected struct RedisConfig
        {
            internal static RedisConfig Create() => new RedisConfig(
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

            internal Dictionary<string, string> Wrapped { get; }
            public int Count => Wrapped.Count;

            private RedisConfig(Dictionary<string, string> inner) => Wrapped = inner;
            public string this[string key]
            {
                get => Wrapped.TryGetValue(key, out var val) ? val : null;
                set
                {
                    if (Wrapped.ContainsKey(key)) Wrapped[key] = value; // no need to fix case
                    else Wrapped[key.ToLowerInvariant()] = value;
                }
            }

            internal int CountMatch(string pattern)
            {
                int count = 0;
                foreach (var pair in Wrapped)
                {
                    if (IsMatch(pattern, pair.Key)) count++;
                }
                return count;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(3, "config", "get", LockFree = true)]
        protected virtual TypedRedisValue Config(RedisClient client, RedisRequest request)
        {
            var pattern = request.GetString(2);

            OnUpdateServerConfiguration();
            var config = ServerConfiguration;
            var matches = config.CountMatch(pattern);
            if (matches == 0) return TypedRedisValue.EmptyArray;

            var arr = TypedRedisValue.Rent(2 * matches, out var span);
            int index = 0;
            foreach (var pair in config.Wrapped)
            {
                if (IsMatch(pattern, pair.Key))
                {
                    span[index++] = TypedRedisValue.BulkString(pair.Key);
                    span[index++] = TypedRedisValue.BulkString(pair.Value);
                }
            }
            if (index != span.Length)
            {
                arr.Recycle(index);
                throw new InvalidOperationException("Configuration CountMatch fail");
            }
            return arr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2, LockFree = true)]
        protected virtual TypedRedisValue Echo(RedisClient client, RedisRequest request)
            => TypedRedisValue.BulkString(request.GetValue(1));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2)]
        protected virtual TypedRedisValue Exists(RedisClient client, RedisRequest request)
        {
            int count = 0;
            var db = client.Database;
            for (int i = 1; i < request.Count; i++)
            {
                if (Exists(db, request.GetKey(i)))
                    count++;
            }
            return TypedRedisValue.Integer(count);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual bool Exists(int database, RedisKey key)
        {
            try
            {
                return !Get(database, key).IsNull;
            }
            catch (InvalidCastException) { return true; } // to be an invalid cast, it must exist
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2)]
        protected virtual TypedRedisValue Get(RedisClient client, RedisRequest request)
            => TypedRedisValue.BulkString(Get(client.Database, request.GetKey(1)));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual RedisValue Get(int database, RedisKey key) => throw new NotSupportedException();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(3)]
        protected virtual TypedRedisValue Set(RedisClient client, RedisRequest request)
        {
            Set(client.Database, request.GetKey(1), request.GetValue(2));
            return TypedRedisValue.OK;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected virtual void Set(int database, RedisKey key, RedisValue value) => throw new NotSupportedException();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(1)]
        protected new virtual TypedRedisValue Shutdown(RedisClient client, RedisRequest request)
        {
            DoShutdown(ShutdownReason.ClientInitiated);
            return TypedRedisValue.OK;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2)]
        protected virtual TypedRedisValue Strlen(RedisClient client, RedisRequest request)
            => TypedRedisValue.Integer(Strlen(client.Database, request.GetKey(1)));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual long Strlen(int database, RedisKey key) => Get(database, key).Length();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-2)]
        protected virtual TypedRedisValue Del(RedisClient client, RedisRequest request)
        {
            int count = 0;
            for (int i = 1; i < request.Count; i++)
            {
                if (Del(client.Database, request.GetKey(i)))
                    count++;
            }
            return TypedRedisValue.Integer(count);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual bool Del(int database, RedisKey key) => throw new NotSupportedException();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(1)]
        protected virtual TypedRedisValue Dbsize(RedisClient client, RedisRequest request)
            => TypedRedisValue.Integer(Dbsize(client.Database));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        protected virtual long Dbsize(int database) => throw new NotSupportedException();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(1)]
        protected virtual TypedRedisValue Flushall(RedisClient client, RedisRequest request)
        {
            var count = Databases;
            for (int i = 0; i < count; i++)
            {
                Flushdb(i);
            }
            return TypedRedisValue.OK;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(1)]
        protected virtual TypedRedisValue Flushdb(RedisClient client, RedisRequest request)
        {
            Flushdb(client.Database);
            return TypedRedisValue.OK;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        protected virtual void Flushdb(int database) => throw new NotSupportedException();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-1, LockFree = true, MaxArgs = 2)]
        protected virtual TypedRedisValue Info(RedisClient client, RedisRequest request)
        {
            var info = Info(request.Count == 1 ? null : request.GetString(1));
            return TypedRedisValue.BulkString(info);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        protected virtual string Info(string selected)
        {
            var sb = new StringBuilder();
            bool IsMatch(string section) => string.IsNullOrWhiteSpace(selected)
                || string.Equals(section, selected, StringComparison.OrdinalIgnoreCase);
            if (IsMatch("Server")) Info(sb, "Server");
            if (IsMatch("Clients")) Info(sb, "Clients");
            if (IsMatch("Memory")) Info(sb, "Memory");
            if (IsMatch("Persistence")) Info(sb, "Persistence");
            if (IsMatch("Stats")) Info(sb, "Stats");
            if (IsMatch("Replication")) Info(sb, "Replication");
            if (IsMatch("Keyspace")) Info(sb, "Keyspace");
            return sb.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2)]
        protected virtual TypedRedisValue Keys(RedisClient client, RedisRequest request)
        {
            List<TypedRedisValue> found = null;
            foreach (var key in Keys(client.Database, request.GetKey(1)))
            {
                if (found == null) found = new List<TypedRedisValue>();
                found.Add(TypedRedisValue.BulkString(key.AsRedisValue()));
            }
            if (found == null) return TypedRedisValue.EmptyArray;
            return TypedRedisValue.MultiBulk(found);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        protected virtual IEnumerable<RedisKey> Keys(int database, RedisKey pattern) => throw new NotSupportedException();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="section"></param>
        protected virtual void Info(StringBuilder sb, string section)
        {
            StringBuilder AddHeader()
            {
                if (sb.Length != 0) sb.AppendLine();
                return sb.Append("# ").AppendLine(section);
            }

            switch (section)
            {
                case "Server":
                    AddHeader().AppendLine("redis_version:1.0")
                        .AppendLine("redis_mode:standalone")
                        .Append("os:").Append(Environment.OSVersion).AppendLine()
                        .Append("arch_bits:x").Append(IntPtr.Size * 8).AppendLine();
                    using (var process = Process.GetCurrentProcess())
                    {
                        sb.Append("process:").Append(process.Id).AppendLine();
                    }
                    //var port = TcpPort();
                    //if (port >= 0) sb.Append("tcp_port:").Append(port).AppendLine();
                    break;
                case "Clients":
                    AddHeader().Append("connected_clients:").Append(ClientCount).AppendLine();
                    break;
                case "Memory":
                    break;
                case "Persistence":
                    AddHeader().AppendLine("loading:0");
                    break;
                case "Stats":
                    AddHeader().Append("total_connections_received:").Append(TotalClientCount).AppendLine()
                        .Append("total_commands_processed:").Append(TotalCommandsProcesed).AppendLine();
                    break;
                case "Replication":
                    AddHeader().AppendLine("role:master");
                    break;
                case "Keyspace":
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2, "memory", "purge")]
        protected virtual TypedRedisValue MemoryPurge(RedisClient client, RedisRequest request)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            return TypedRedisValue.OK;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-2)]
        protected virtual TypedRedisValue Mget(RedisClient client, RedisRequest request)
        {
            int argCount = request.Count;
            var arr = TypedRedisValue.Rent(argCount - 1, out var span);
            var db = client.Database;
            for (int i = 1; i < argCount; i++)
            {
                span[i - 1] = TypedRedisValue.BulkString(Get(db, request.GetKey(i)));
            }
            return arr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-3)]
        protected virtual TypedRedisValue Mset(RedisClient client, RedisRequest request)
        {
            int argCount = request.Count;
            var db = client.Database;
            for (int i = 1; i < argCount;)
            {
                Set(db, request.GetKey(i++), request.GetValue(i++));
            }
            return TypedRedisValue.OK;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-1, LockFree = true, MaxArgs = 2)]
        protected virtual TypedRedisValue Ping(RedisClient client, RedisRequest request)
            => TypedRedisValue.SimpleString(request.Count == 1 ? "PONG" : request.GetString(1));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(1, LockFree = true)]
        protected virtual TypedRedisValue Quit(RedisClient client, RedisRequest request)
        {
            RemoveClient(client);
            return TypedRedisValue.OK;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(1, LockFree = true)]
        protected virtual TypedRedisValue Role(RedisClient client, RedisRequest request)
        {
            var arr = TypedRedisValue.Rent(3, out var span);
            span[0] = TypedRedisValue.BulkString("master");
            span[1] = TypedRedisValue.Integer(0);
            span[2] = TypedRedisValue.EmptyArray;
            return arr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2, LockFree = true)]
        protected virtual TypedRedisValue Select(RedisClient client, RedisRequest request)
        {
            Console.WriteLine("=== 选择数据库 {0} -> {1}", client.Database, request.GetValue(1).ToString());
            var raw = request.GetValue(1);
            var val = raw.ToString();
            long lval = -1;
            long.TryParse(val,out lval);
            if (lval<0)
            {
                Console.WriteLine("== 选择数据库 不是一个索引");
                return TypedRedisValue.Error("ERR invalid DB index");
            }
            int db = (int)raw;
            if (db < 0 || db >= Databases)
            {
                Console.WriteLine("== 选择数据库 索引超出");
                return TypedRedisValue.Error("ERR DB index is out of range");
            }
            client.Database = db;
            Console.WriteLine("== 选择数据库"+ db);
            return TypedRedisValue.OK;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-2)]
        protected virtual TypedRedisValue Subscribe(RedisClient client, RedisRequest request)
            => SubscribeImpl(client, request);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-2)]
        protected virtual TypedRedisValue Unsubscribe(RedisClient client, RedisRequest request)
            => SubscribeImpl(client, request);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private TypedRedisValue SubscribeImpl(RedisClient client, RedisRequest request)
        {
            var reply = TypedRedisValue.Rent(3 * (request.Count - 1), out var span);
            int index = 0;
            request.TryGetCommandBytes(0, out var cmd);
            var cmdString = TypedRedisValue.BulkString(cmd.ToArray());
            var mode = cmd[0] == (byte)'p' ? RedisChannel.PatternMode.Pattern : RedisChannel.PatternMode.Literal;
            for (int i = 1; i < request.Count; i++)
            {
                var channel = request.GetChannel(i, mode);
                int count;
                if (s_Subscribe.Equals(cmd))
                {
                    count = client.Subscribe(channel);
                }
                else if (s_Unsubscribe.Equals(cmd))
                {
                    count = client.Unsubscribe(channel);
                }
                else
                {
                    reply.Recycle(index);
                    return TypedRedisValue.Nil;
                }
                span[index++] = cmdString;
                span[index++] = TypedRedisValue.BulkString((byte[])channel);
                span[index++] = TypedRedisValue.Integer(count);
            }
            return reply;
        }
        /// <summary>
        /// 
        /// </summary>
        private static readonly CommandBytes
            s_Subscribe = new CommandBytes("subscribe"),
            s_Unsubscribe = new CommandBytes("unsubscribe");
        /// <summary>
        /// 
        /// </summary>
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(1, LockFree = true)]
        protected virtual TypedRedisValue Time(RedisClient client, RedisRequest request)
        {
            var delta = Time() - UnixEpoch;
            var ticks = delta.Ticks;
            var seconds = ticks / TimeSpan.TicksPerSecond;
            var micros = (ticks % TimeSpan.TicksPerSecond) / (TimeSpan.TicksPerMillisecond / 1000);
            var reply = TypedRedisValue.Rent(2, out var span);
            span[0] = TypedRedisValue.BulkString(seconds);
            span[1] = TypedRedisValue.BulkString(micros);
            return reply;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual DateTime Time() => DateTime.UtcNow;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(-2)]
        protected virtual TypedRedisValue Unlink(RedisClient client, RedisRequest request)
            => Del(client, request);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2)]
        protected virtual TypedRedisValue Incr(RedisClient client, RedisRequest request)
            => TypedRedisValue.Integer(IncrBy(client.Database, request.GetKey(1), 1));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(2)]
        protected virtual TypedRedisValue Decr(RedisClient client, RedisRequest request)
            => TypedRedisValue.Integer(IncrBy(client.Database, request.GetKey(1), -1));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [RedisCommand(3)]
        protected virtual TypedRedisValue IncrBy(RedisClient client, RedisRequest request)
            => TypedRedisValue.Integer(IncrBy(client.Database, request.GetKey(1), request.GetInt64(2)));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        protected virtual long IncrBy(int database, RedisKey key, long delta)
        {
            var value = ((long)Get(database, key)) + delta;
            Set(database, key, value);
            return value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace WS_Core.Bodys.Redis
{
    /// <summary>
    /// Redis 信息
    /// </summary>
    [Author("Linyee", "2019-06-19")]
    public class RedisInfoV1_0
    {
        /// <summary>
        /// 默认实例
        /// </summary>
        public static RedisInfoV1_0 Default = new RedisInfoV1_0();

        #region "服务器信息"

        /// <summary>
        /// 所有信息
        /// </summary>
        public string All => GetAll();

        private string GetAll()
        {
            StringBuilder sbd = new StringBuilder();
            sbd.AppendLine(GetValue("server"));
            sbd.AppendLine(GetValue("clients"));
            sbd.AppendLine(GetValue("persistence"));
            sbd.AppendLine(GetValue("stats"));
            sbd.AppendLine(GetValue("replication"));
            return sbd.ToString();
        }

        /// <summary>
        /// 信息
        /// </summary>
        public string server { get; set; } = @"# Server
redis_version:1.0
redis_mode:standalone
os:Microsoft Windows NT 6.2.9200.0
arch_bits:x64
process:8272
";

        /// <summary>
        /// 信息
        /// </summary>
        public string clients { get; set; } = @"# Clients
connected_clients:2
";

        /// <summary>
        /// 信息
        /// </summary>
        public string persistence { get; set; } = @"# Persistence
loading:0
";

        /// <summary>
        /// 信息
        /// </summary>
        public string stats { get; set; } = @"# Stats
total_connections_received:2
total_commands_processed:8
";

        /// <summary>
        /// 信息
        /// </summary>
        public string replication { get; set; } = @"# Replication
role:master
";

        #endregion

        /// <summary>
        /// 获取指定属性
        /// </summary>
        /// <param name="key"></param>
         /// <param name="key2"></param>
       /// <returns></returns>
        public ExecuteResult<string> GetValue(string key, string key2 = null)
        {
            var realkey = key.Replace("-", "_");
            ExecuteResult<string> result = new ExecuteResult<string>();
            var p = this.GetType().GetProperty(realkey);
            if (p == null) return result.SetFail(key + "未找到指定对象名称，注意区分大小写");
            var val = p.GetValue(this);
            var valType = val.GetType();
            //子键
            if (valType == typeof(RedisInfoCluster) && !string.IsNullOrEmpty(key2))
            {
                var p2 = valType.GetProperty(key2);
                if (p2 == null) return result.SetFail(key2 + "未找到指定对象名称，注意区分大小写");
                val = p2.GetValue(this);
            }

            result.SetOk().SetData(val.ToString());
            return result;
        }
    }


    /// <summary>
    /// Redis 信息
    /// </summary>
    [Author("Linyee", "2019-06-19")]
    public class RedisInfo
    {
        /// <summary>
        /// 默认实例
        /// </summary>
        public static RedisInfo Default = new RedisInfo();

        #region "服务器信息"

        /*
# Server
redis_version:1.0
redis_mode:standalone
os:Microsoft Windows NT 6.2.9200.0
arch_bits:x64
process:8272

# Clients
connected_clients:2

# Persistence
loading:0

# Stats
total_connections_received:2
total_commands_processed:8

# Replication
role:master
*/

        /// <summary>
        /// 所有信息
        /// </summary>
        public string All => GetAll();

        private string GetAll()
        {
            StringBuilder sbd = new StringBuilder();
            sbd.AppendLine(GetValue("server"));
            sbd.AppendLine(GetValue("clients"));
            sbd.AppendLine(GetValue("memory"));
            sbd.AppendLine(GetValue("persistence"));
            sbd.AppendLine(GetValue("stats"));
            sbd.AppendLine(GetValue("replication"));
            sbd.AppendLine(GetValue("cpu"));
            sbd.AppendLine(GetValue("cluster"));
            sbd.AppendLine(GetValue("keyspace"));
            return sbd.ToString();
        }

        /// <summary>
        /// 信息
        /// </summary>
        public string server { get; set; } = @"# Server
redis_version:1.0
redis_mode:standalone
os:Microsoft Windows NT 6.2.9200.0
arch_bits:x64
process:8272
";
        /* @"# Server
redis_version:3.2.100
redis_git_sha1:00000000
redis_git_dirty:0
redis_build_id:dd26f1f93c5130ee
redis_mode:standalone
os:Windows  
arch_bits:64
multiplexing_api:WinSock_IOCP
process_id:9700
run_id:9e328efe0e09159409459d4bed23c1d422464b95
tcp_port:6379
uptime_in_seconds:10178
uptime_in_days:0
hz:10
lru_clock:9559253
executable:E:\_Redis\Redis-x64-3.2.100\redis-server.exe
config_file:E:\_Redis\Redis-x64-3.2.100\redis.windows.conf
";
*/

        /// <summary>
        /// 信息
        /// </summary>
        public string clients { get; set; } = @"# Clients
connected_clients:2
client_longest_output_list:3
client_biggest_input_buf:116
blocked_clients:0
";

        /// <summary>
        /// 信息
        /// </summary>
        public string memory { get; set; } = @"# Memory
used_memory:747816
used_memory_human:730.29K
used_memory_rss:630608
used_memory_rss_human:615.83K
used_memory_peak:867328
used_memory_peak_human:847.00K
total_system_memory:0
total_system_memory_human:0B
used_memory_lua:37888
used_memory_lua_human:37.00K
maxmemory:0
maxmemory_human:0B
maxmemory_policy:noeviction
mem_fragmentation_ratio:0.84
mem_allocator:jemalloc-3.6.0
";

        /// <summary>
        /// 信息
        /// </summary>
        public string persistence { get; set; } = @"# Persistence
loading:0
rdb_changes_since_last_save:0
rdb_bgsave_in_progress:0
rdb_last_save_time:1553052947
rdb_last_bgsave_status:ok
rdb_last_bgsave_time_sec:-1
rdb_current_bgsave_time_sec:-1
aof_enabled:0
aof_rewrite_in_progress:0
aof_rewrite_scheduled:0
aof_last_rewrite_time_sec:-1
aof_current_rewrite_time_sec:-1
aof_last_bgrewrite_status:ok
aof_last_write_status:ok
";

        /// <summary>
        /// 信息
        /// </summary>
        public string stats { get; set; } = @"# Stats
total_connections_received:56
total_commands_processed:293
instantaneous_ops_per_sec:0
total_net_input_bytes:15872
total_net_output_bytes:77718
instantaneous_input_kbps:0.00
instantaneous_output_kbps:0.00
rejected_connections:0
sync_full:0
sync_partial_ok:0
sync_partial_err:0
expired_keys:0
evicted_keys:0
keyspace_hits:0
keyspace_misses:0
pubsub_channels:1
pubsub_patterns:0
latest_fork_usec:0
migrate_cached_sockets:0
";

        /// <summary>
        /// 信息
        /// </summary>
        public string replication { get; set; } = @"# Replication
role:master
connected_slaves:0
master_repl_offset:0
repl_backlog_active:0
repl_backlog_size:1048576
repl_backlog_first_byte_offset:0
repl_backlog_histlen:0
";

        /// <summary>
        /// 信息
        /// </summary>
        public string cpu { get; set; } = @"# CPU
used_cpu_sys:0.14
used_cpu_user:0.30
used_cpu_sys_children:0.00
used_cpu_user_children:0.00
";

        /// <summary>
        /// 信息
        /// </summary>
        public string cluster { get; set; } = @"# Cluster
cluster_enabled:0
";

        /// <summary>
        /// 信息
        /// </summary>
        public string keyspace { get; set; } = @"# Keyspace
db0:keys=1,expires=0,avg_ttl=0
";
        #endregion

        /// <summary>
        /// 获取指定属性
        /// </summary>
        /// <param name="key"></param>
          /// <param name="key2"></param>
      /// <returns></returns>
        public ExecuteResult<string> GetValue(string key, string key2 = null)
        {
            var realkey = key.Replace("-", "_");
            ExecuteResult<string> result = new ExecuteResult<string>();
            var p = this.GetType().GetProperty(realkey);
            if (p == null) return result.SetFail(key + "未找到指定对象名称，注意区分大小写");
            var val = p.GetValue(this);
            var valType = val.GetType();
            //子键
            if (valType == typeof(RedisInfoCluster) && !string.IsNullOrEmpty(key2))
            {
                var p2 = valType.GetProperty(key2);
                if (p2 == null) return result.SetFail(key2 + "未找到指定对象名称，注意区分大小写");
                val = p2.GetValue(this);
            }

            result.SetOk().SetData(val.ToString());
            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RedisInfoCluster
    {
        /// <summary>
        /// 默认实例
        /// </summary>
        public static RedisInfoCluster Default = new RedisInfoCluster();

        /// <summary>
        /// 节点
        /// </summary>

        public string[] NODES = new string[] { };
    }
}

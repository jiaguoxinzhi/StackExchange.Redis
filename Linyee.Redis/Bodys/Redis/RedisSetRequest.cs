using System;
using System.Collections.Generic;
using System.Text;
using WS_Core.Bodys.Json;

namespace WS_Core.Bodys.Redis
{
    /// <summary>
    /// Redis Set 请求
    /// </summary>
    [Author("Linyee", "2019-01-31")]
    public class RedisSetRequest: QuestBodyBase
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 过期秒数
        /// </summary>
        public long ExpireSec { get; set; }
    }
}

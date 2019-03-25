using System;
using System.Collections.Generic;
using System.Text;

namespace WS_Core.Bodys.Redis
{
    /// <summary>
    /// Redis Expire 请求
    /// </summary>
    [Author("Linyee", "2019-01-31")]
    public class RedisExpireRequest: QuestBodyBase
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 过期秒数
        /// </summary>
        public long ExpireSec { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace WS_Core.Bodys.Redis
{
    /// <summary>
    /// Redis Get 请求
    /// </summary>
    [Author("Linyee", "2019-01-31")]
    public class RedisGetRequest : QuestBodyBase
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; }
    }
}

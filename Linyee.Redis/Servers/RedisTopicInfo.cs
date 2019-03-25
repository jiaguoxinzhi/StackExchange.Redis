using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WS_Server.Servers
{
    /// <summary>
    /// 主题信息
    /// </summary>
    public class RedisTopicInfo
    {
        /// <summary>
        /// 主题
        /// </summary>
        public string Topic{get;set;}

        /// <summary>
        /// 类别代码
        /// 0任何人都可以访问 &gt;=1000000000000（万亿）为系统主题 &lt;1000000000000（万亿）为用户主题
        /// </summary>
        public long TypeCode { get; set; } = 0;

        /// <summary>
        /// 主类型
        /// </summary>
        [JsonIgnore]
        public int MainTypeCode =>(int)( TypeCode % 100);

        /// <summary>
        /// 创建人 长ID
        /// 即房主
        /// </summary>
        public long LongId { get; set; }

        /// <summary>
        /// 关注人id集
        /// </summary>
        public List<long> SubIds { get; set; } = new List<long>();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后收发消息时间
        /// </summary>
        public DateTime LastMsgTime { get; set; }

        /// <summary>
        /// 检测
        /// </summary>
        /// <returns></returns>
        public ExecuteResult<bool> Check()
        {
            ExecuteResult<bool> result = new ExecuteResult<bool>();
            if (Topic?.StartsWith("/") != true) return result.SetFail("主题、频道必须以/开头").SetData(false);

            return result.SetOk().SetData(true);
        }
    }
}

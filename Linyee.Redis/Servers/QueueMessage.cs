using System;
using System.Collections.Generic;
using System.Text;

namespace WS_Server.Servers
{
    /// <summary>
    /// 队列消息
    /// </summary>
    [Author("Linyee", "2019-3-11")]
    public class QueueMessage
    {
        /// <summary>
        /// 队列消息
        /// </summary>
        public QueueMessage() { }

        /// <summary>
        /// 发用户
        /// </summary>
        public long u { get; internal set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string m { get; internal set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public long t { get; internal set; }
        /// <summary>
        /// 指定收用户
        /// 0 表示不限定用户
        /// </summary>
        public long d { get; internal set; }
        /// <summary>
        /// 主题
        /// </summary>
        public string p { get; internal set; }
    }
}

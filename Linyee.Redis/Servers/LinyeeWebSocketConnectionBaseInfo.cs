using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WS_Server.Servers
{
    /// <summary>
    /// WebSokcet连接信息
    /// Linyee 2018-06-03
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public partial class LinyeeWebSocketConnectionBaseInfo
    {

        #region "属性"
        /// <summary>
        /// UUID
        /// </summary>
        public Guid ClientId { get; protected set; }

        /// <summary>
        /// 类别代码
        /// </summary>
        public int TypeCode { get; protected set; }

        /// <summary>
        /// 主类型
        /// </summary>
        [JsonIgnore]
        public int MainTypeCode => TypeCode % 100;

        /// <summary>
        /// 长ID
        /// 设置值时必须在TypeCode之后
        /// </summary>
        public long LongId {get;set;}

        /// <summary>
        /// 短ID
        /// </summary>
        public int Id { get; protected internal set; }

        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime ConnectedTime { get; protected set; }

        /// <summary>
        /// 最后收发消息时间
        /// </summary>
        public DateTime LastMsgTime { get; protected internal set; }

        /// <summary>
        /// 客户端名称
        /// </summary>
        public string Name { get; internal protected set; }

        /// <summary>
        /// 是否已认证
        /// </summary>
        public bool Authed { get; set; }

        /// <summary>
        /// 泛关注的主题
        /// </summary>
        public string Topic { get; internal set; }

        /// <summary>
        /// 泛关注的正则
        /// </summary>
        [JsonIgnore]
        public Regex TopicRegex =>string.IsNullOrEmpty(Topic)?null: new Regex(Topic, RegexOptions.Compiled);

        #endregion
    }
}

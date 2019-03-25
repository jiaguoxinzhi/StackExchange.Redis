using System;
using System.Collections.Generic;
using System.Text;
using WS_Core.Bodys;

namespace WS_Server.Servers
{

    /// <summary>
    /// 事务信息
    /// </summary>
    [Author("Linyee", "2019-02-01")]
    public class RedisMultiInfo
    {

        /// <summary>
        /// 事务脚本
        /// </summary>
        public Func<LinyeeWebSocketConnectionBase, RedisCachingInfo> MultiFunc;
        /// <summary>
        /// id
        /// </summary>
        public long LongId;
        /// <summary>
        /// 主类别
        /// </summary>
        public int MainTypeCode;
        /// <summary>
        /// 时间点
        /// </summary>
        public long Ticks = DateTime.Now.Ticks;

        /// <summary>
        /// 事务信息
        /// </summary>
        /// <param name="longId"></param>
        /// <param name="mainTypeCode"></param>
        public RedisMultiInfo(long longId, int mainTypeCode)
        {
            LongId = longId;
            MainTypeCode = mainTypeCode;
        }

        /// <summary>
        /// 事务信息
        /// </summary>
        /// <param name="longId"></param>
        /// <param name="mainTypeCode"></param>
        /// <param name="mf">事务脚本</param>
        public RedisMultiInfo(long longId, int mainTypeCode, Func<LinyeeWebSocketConnectionBase, RedisCachingInfo> mf) : this(longId, mainTypeCode)
        {
            MultiFunc = mf;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Key => "" + LongId + MainTypeCode + Ticks;
    }
}

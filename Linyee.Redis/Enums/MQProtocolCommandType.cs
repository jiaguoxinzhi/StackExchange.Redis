using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WS_Core.Enums
{
    /// <summary>
    /// 命令
    /// </summary>
    public enum MQProtocolCommandType
    {
        /// <summary>
        /// 未指定命令
        /// </summary>
        NONE=0,
        /// <summary>
        /// 获取支付地址
        /// </summary>
        GetPayUrl=1,
        /// <summary>
        /// 登录
        /// </summary>
        Login = 2,

        #region ActionRedis=3
        /// <summary>
        /// 
        /// </summary>
        GET = 300, //获取一个key的值
                   /// <summary>
                   /// 
                   /// </summary>
        INFO, //Redis信息。  
              /// <summary>
              /// 
              /// </summary>
        SET, //添加一个值
             /// <summary>
             /// 
             /// </summary>
        EXPIRE, //设置过期时间
                /// <summary>
                /// 
                /// </summary>
        MULTI, //标记一个事务块开始
               /// <summary>
               /// 
               /// </summary>
        EXEC, //执行所有 MULTI 之后发的命令

        /// <summary>
        /// 
        /// </summary>
        EXISTS,//是否存在
               /// <summary>
               /// 
               /// </summary>
        QUIT,
        /// <summary>
        /// 
        /// </summary>
        SUBSCRIBE,
        /// <summary>
        /// 
        /// </summary>
        UNSUBSCRIBE,
        /// <summary>
        /// 
        /// </summary>
        PSUBSCRIBE,
        /// <summary>
        /// 
        /// </summary>
        PUNSUBSCRIBE,
        /// <summary>
        /// 
        /// </summary>
        PUBLISH,
        /// <summary>
        /// 
        /// </summary>
        PUBSUB,
        /// <summary>
        /// 
        /// </summary>
        AUTH,
        /// <summary>
        /// 
        /// </summary>
        PING,
        /// <summary>
        /// 
        /// </summary>
        DBSIZE,
        /// <summary>
        /// 
        /// </summary>
        DEL,
        /// <summary>
        /// 
        /// </summary>
        SELECT,
        /// <summary>
        /// 
        /// </summary>
        DISCARD,
        /// <summary>
        /// 
        /// </summary>
        SAVE,
        /// <summary>
        /// 
        /// </summary>
        EVAL,
        /// <summary>
        /// 
        /// </summary>
        ENQ,
        /// <summary>
        /// 
        /// </summary>
        DEQ,
        /// <summary>
        /// 
        /// </summary>
        PEEKQ,
        /// <summary>
        /// 
        /// </summary>
        LENQ,
        /// <summary>
        /// 
        /// </summary>
        LPUSH,
        /// <summary>
        /// 
        /// </summary>
        LPOP,
        /// <summary>
        /// 
        /// </summary>
        LLEN
        #endregion

    }
}

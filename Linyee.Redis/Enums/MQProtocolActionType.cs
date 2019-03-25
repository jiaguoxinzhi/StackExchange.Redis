using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WS_Core.Enums
{
    /// <summary>
    /// 请求的动作类型
    /// </summary>
    public enum MQProtocolActionType
    {
        /// <summary>
        /// 未指明
        /// </summary>
        NONE=0,
        /// <summary>
        /// 请求
        /// </summary>
        Request=1,
        /// <summary>
        /// 响应
        /// </summary>
        Response=2,
        /// <summary>
        /// Redis
        /// </summary>
        Redis = 3,
        /// <summary>
        /// 错误
        /// </summary>
        ERROR = 10,
    }
}

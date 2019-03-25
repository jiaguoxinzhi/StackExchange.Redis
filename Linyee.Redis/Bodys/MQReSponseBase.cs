using System;
using System.Collections.Generic;
using System.Text;
using WS_Core.Enums;

namespace WS_Core.Bodys
{
    #region 响应 基础

    /// <summary>
    /// 响应 基础
    /// </summary>
    [Author("Linyee", "2019-01-28")]
    public class MQReSponseBase : MQReQuestSponseBase
    {
        /// <summary>
        /// 
        /// </summary>
        public MQReSponseBase()
        {
            Body = new object();
        }

        /// <summary>
        /// 状态码
        /// </summary>
        public StatusCodeEnum StatusCode;
        /// <summary>
        /// 状态名
        /// </summary>
        public string StatusName { get { return (StatusCode).ToString().Replace("_", " "); } }
        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDescription { get { return (ConstEnum.GetEnumDescription(StatusCode)); } }

        /// <summary>
        /// 状态详情
        /// </summary>
        public string StatusDetails { get; set; }

    }

    /// <summary>
    /// 响应 基础
    /// </summary>
    [Author("Linyee", "2019-01-28")]
    public class MQReSponseBase<T> : MQReQuestSponseBase<T>
        where T : SponseBodyBase, new()
    {
        /// <summary>
        /// 
        /// </summary>
        public MQReSponseBase()
        {
            Body = new T();
        }

        /// <summary>
        /// 状态码
        /// </summary>
        public StatusCodeEnum StatusCode;
        /// <summary>
        /// 状态名
        /// </summary>
        public string StatusName { get { return StatusCode.ToString().Replace("_", " "); } }
        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDescription { get { return (ConstEnum.GetEnumDescription(StatusCode)); } }

        /// <summary>
        /// 响应超时
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        internal static MQReSponseBase<T> Timeout(int timeout)
        {
            return new MQReSponseBase<T>()
            {
                StatusCode = StatusCodeEnum.Request_Timeout,
            };
        }
    }

    #endregion
}

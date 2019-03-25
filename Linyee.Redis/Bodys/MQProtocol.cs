using System;
using System.Collections.Generic;
using System.Text;
using WS_Core.Bodys.Json;
using WS_Core.Enums;

namespace WS_Core.Bodys
{
    /// <summary>
    /// 队列消息
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class MQProtocol : MQProtocol<MQDataBase>
    {
        /// <summary>
        /// 构造
        /// </summary>
        public MQProtocol() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="quest"></param>
        public MQProtocol(PayRequest quest)
        {
            this.Data = new MQReQuestBase<PayRequest>()
            {
                path = "/GetPayUrl",
                Body = quest,
            };
            this.Action = MQProtocolActionType.Request;
            this.Command = MQProtocolCommandType.GetPayUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quest"></param>
        public MQProtocol(PayResponse quest)
        {
            this.Data = new MQReSponseBase<PayResponse>()
            {
                Body = quest,
            };
            this.Action = MQProtocolActionType.Response;
            this.Command = MQProtocolCommandType.GetPayUrl;
        }
    }

    /// <summary>
    /// 队列消息
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class MQQSProtocol : MQQSProtocol<MQReQuestSponseBase>
    {
        /// <summary>
        /// 构造
        /// </summary>
        public MQQSProtocol() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quest"></param>
        public MQQSProtocol(PayRequest quest)
        {
            this.Data = new MQReQuestBase<PayRequest>()
            {
                path = "/GetPayUrl",
                Body = quest,
            };
            this.Action = MQProtocolActionType.Request;
            this.Command = MQProtocolCommandType.GetPayUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quest"></param>
        public MQQSProtocol(PayResponse quest)
        {
            this.Data = new MQReSponseBase<PayResponse>()
            {
                Body = quest,
            };
            this.Action = MQProtocolActionType.Response;
            this.Command = MQProtocolCommandType.GetPayUrl;
        }
    }



    /// <summary>
    /// 队列消息 协议层
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class MQProtocol<T>
        where T : MQDataBase, new()
    {
        /// <summary>
        /// 构造
        /// </summary>
        public MQProtocol()
        {
            Data = new T();
        }

        /// <summary>
        /// 请求的动作
        /// </summary>
        public MQProtocolActionType Action { get; set; }

        /// <summary>
        /// 请求的命令
        /// </summary>
        public MQProtocolCommandType Command { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }
    }

    /// <summary>
    /// 队列消息
    /// 请求响应
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class MQQSProtocol<T> : MQProtocol<T>
        where T : MQReQuestSponseBase, new()
    {
        /// <summary>
        /// 构造
        /// </summary>
        public MQQSProtocol() { }

        /// <summary>
        /// 数据
        /// </summary>
        public new T Data { get; set; }
    }
}

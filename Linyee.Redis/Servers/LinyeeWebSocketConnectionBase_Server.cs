using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WS_Core.Bodys;
using WS_Core.Bodys.Json;
using WS_Core.Enums;

namespace WS_Server.Servers
{
    /// <summary>
    /// 服务端消息
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public static class LinyeeWebSocketConnectionBase_Server
    {
        //public ConcurrentQueue<string> RequestIdQueue = new ConcurrentQueue<string>();

        #region "发送消息"
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <param name="endOfMessage"></param>
        /// <param name="messageType"></param>
        public static async Task SendMsg(this LinyeeWebSocketConnectionBase client, string msg,bool endOfMessage = true, WebSocketMessageType messageType= WebSocketMessageType.Text)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
            try
            {
                await client.Client.SendAsync(buffer, messageType, endOfMessage, client.CancelToken);
                LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "发送消息：" + msg.Replace("\r\n","\\r\\n"));
                client.LastMsgTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                LogService.Exception(ex);
                LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "发送失败：" + msg.Replace("\r\n", "\\r\\n"));
                client.Dispose();
            }
        }

        /// <summary>
        /// 发送一个成功数据
        /// </summary>
        public static async Task SendMsgOK(this LinyeeWebSocketConnectionBase client)
        {
            await client.SendMsg("OK");
        }

        /// <summary>
        /// 发送一个失败数据
        /// </summary>
        public static async Task SendMsgFail(this LinyeeWebSocketConnectionBase client)
        {
            await client.SendMsg("FAIL");
        }
        #endregion

        #region "服务端功能"

        #region "同步获取支付链接"

        /// <summary>
        /// 获取支付地址
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="money"></param>
        /// <param name="mark"></param>
        /// <param name="product"></param>
        /// <param name="timestamp"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public static async Task<PayResponse> GetPayAsync(this LinyeeWebSocketConnectionBase client, long id, string type, decimal money, string mark, string product, long timestamp, string sign)
        {
            var quest = new PayRequest(id, type, money, mark, product, timestamp,sign);
            return await client.GetPayAsync(quest);
        }

        /// <summary>
        /// 获取支付地址
        /// </summary>
        /// <param name="client"></param>
        /// <param name="quest"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<PayResponse> GetPayAsync(this LinyeeWebSocketConnectionBase client, PayRequest quest, int timeout = 90)
        {
            var ResponseMsg = client.ResponseMsg;

            var mqdata = new MQQSProtocol(quest);
            //确保无重复请求ID
            while (ResponseMsg.ContainsKey(mqdata.Data.RequestId))
            {
                mqdata.Data.RequestId = Guid.NewGuid().ToString("N");
            }
            var json = await mqdata.ToJsonStringAsync();
            LogService.AnyLog("ReQuestSponse", "向APP端提交参数：", json);
            await client.SendMsg(json);
            client.LastRequestId = mqdata.Data.RequestId;
            //RequestIdQueue.Enqueue(mqdata.Data.RequestId);
            PayResponse sponse = await client.GetResponse(mqdata.Data.RequestId, timeout);
            //var gid = "";
            //RequestIdQueue.TryPeek(out gid);
            //if (gid == mqdata.Data.RequestId) RequestIdQueue.TryDequeue(out gid);
            return sponse;
        }

        /// <summary>
        /// 根据请求Id 获取数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="requestId"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static async Task<PayResponse> GetResponse(this LinyeeWebSocketConnectionBase client, string requestId, int timeout = 90)
        {
            var ResponseMsg = client.ResponseMsg;

            return await Task.Run<PayResponse>(() =>
            {
                var dt1 = DateTime.Now;
                while (true)
                {
                    if (ResponseMsg.ContainsKey(requestId))
                    {
                        MQProtocol<MQReSponseBase<PayResponse>> mq = null;
                        ResponseMsg.TryRemove(requestId, out mq);
                        if (mq != null)
                        {
                            return mq.Data.Body;
                        }
                    }

                    var dt2 = DateTime.Now;

                    if ((dt2 - dt1).TotalSeconds > timeout)
                    {
                        //await client.CloseMsg(MQProtocolCommandType.GetPayUrl.ToString() + "超时");
                        return PayResponse.Timeout(timeout);
                    }
                    Thread.Sleep(200);
                }
            });
        }
        #endregion
        #endregion
    }
}

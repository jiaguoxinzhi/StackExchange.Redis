using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WS_Server.SocketServers
{
    /// <summary>
    /// 网络套接字
    /// </summary>
    [Author("Linyee", "2019-03-19")]
    public class LinyeeWebSocket : WebSocket
    {
        /// <summary>
        /// 套接字
        /// </summary>
        private Socket client;
        /// <summary>
        /// 关闭状态
        /// </summary>
        private WebSocketCloseStatus? closeStatus;
        /// <summary>
        /// 关闭描述
        /// </summary>
        private string closeStatusDescription;
        /// <summary>
        /// 状态
        /// </summary>
        private WebSocketState state;
        ///// <summary>
        ///// 子协议
        ///// </summary>
        //private string subProtocol = null;

        /// <summary>
        /// 网络套接字
        /// </summary>
        /// <param name="socket"></param>
        public LinyeeWebSocket(Socket socket)
        {
            this.client = socket;
            if (socket.Connected) state = WebSocketState.Open;
        }

        /// <summary>
        /// 网络套接字
        /// </summary>
        /// <param name="pipe"></param>
        public LinyeeWebSocket(IDuplexPipe pipe)
        {
        }

        /// <summary>
        /// 关闭状态
        /// </summary>
        public override WebSocketCloseStatus? CloseStatus => closeStatus;

        /// <summary>
        /// 关闭描述
        /// </summary>
        public override string CloseStatusDescription => closeStatusDescription;

        /// <summary>
        /// 状态
        /// </summary>
        public override WebSocketState State => state;

        /// <summary>
        /// 子协议
        /// </summary>
        public override string SubProtocol => null;

        /// <summary>
        /// 取消
        /// </summary>
        public override void Abort()
        {
            closeStatus = WebSocketCloseStatus.NormalClosure;
            closeStatusDescription = "取消操作";
            client.Close(90);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="closeStatus"></param>
        /// <param name="statusDescription"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            var tsk = Task.Run(() =>
            {
                this.closeStatus = closeStatus;
                this.closeStatusDescription = statusDescription;
                client.Close(90);
            });
            return tsk;
        }

        /// <summary>
        /// 关闭输出
        /// </summary>
        /// <param name="closeStatus"></param>
        /// <param name="statusDescription"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            var tsk = Task.Run(() =>
            {
                this.state =  WebSocketState.CloseReceived;
            });
            return tsk;
        }

        /// <summary>
        /// 释放
        /// </summary>
        public override void Dispose()
        {
            client.Close(90);
            client.Dispose();
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            if (this.state == WebSocketState.CloseReceived) throw new Exception("当前处于关闭接收状态");
            var tsk = Task.Run(() =>
            {
                var res= client.Receive(buffer.Array, SocketFlags.None);//, cancellationToken
                //{                buffer, SocketFlags.None, cancellationToken                }
                //接收结果
                WebSocketReceiveResult wsres = null;
                if (res == 0)//关闭消息
                {
                    //this.client.Send(new byte[0]);
                    //Console.WriteLine("已关闭 {0}", "aaaaaaaaaa");
                    //wsres = new WebSocketReceiveResult(res, WebSocketMessageType.Close, true);
                    wsres = new WebSocketReceiveResult(res, WebSocketMessageType.Close, true);
                }
                else
                {
                    wsres = new WebSocketReceiveResult(res, WebSocketMessageType.Text, true);
                }
                return wsres;
            });
            return tsk;
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="messageType"></param>
        /// <param name="endOfMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            var tsk = Task.Run(() =>
            {
                //Console.WriteLine(">发送消息");
                //client.SendAsync(buffer, SocketFlags.None, cancellationToken);
                client.Send(buffer.Array, SocketFlags.None);
            });
            return tsk;
        }
    }
}

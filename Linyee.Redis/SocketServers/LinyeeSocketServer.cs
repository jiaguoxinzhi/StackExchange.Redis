//using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using WS_Core.Enums;
using WS_Core.Utils;
using WS_Server.Servers;

namespace WS_Server.SocketServers
{
    /// <summary>
    /// 套接字 服务端
    /// </summary>
    [Author("Linyee", "2019-03-19")]
    public class LinyeeSocketServer
    {
        /// <summary>
        /// 默认实例
        /// </summary>
        public static LinyeeSocketServer Defautl = new LinyeeSocketServer();

        /// <summary>
        /// 连接
        /// </summary>
        private Socket serverSocket;
        /// <summary>
        /// 端口
        /// </summary>
        private int port = 6379;
        /// <summary>
        /// ip地址
        /// </summary>
        private IPAddress ip = IPAddress.Parse("0.0.0.0");
        /// <summary>
        /// 服务线程
        /// </summary>
        private Thread seviceThread = null;

        /// <summary>
        /// 套接字 服务端
        /// </summary>
        public LinyeeSocketServer():this(6379)
        {

        }

        /// <summary>
        /// 套接字 服务端
        /// </summary>
        /// <param name="port"></param>
        public LinyeeSocketServer(int port)
        {
            this.port = port;
            //服务器IP地址 
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, port));  //绑定IP地址：端口 
            serverSocket.Listen(65535);    //设定最多10个排队连接请求 
            //Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            //通过Clientsoket发送数据 
             seviceThread = new Thread(ListenClientConnect);
            //seviceThread.Start();
            //Console.ReadLine();
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public LinyeeSocketServer Start()
        {
            if (seviceThread != null)
            {
                if (seviceThread.ThreadState != ThreadState.Running)
                {
                    seviceThread.Start();
                    Console.WriteLine("{0} 地址{1}:{2}", "Socket服务端启动完成", ip, port);
                }
                else
                {
                    //throw new Exception("服务正在运行，请勿重复启动");
                    Console.WriteLine("{0} 地址{1}:{2}", "Socket服务端已启动", ip, port);
                }
            }
            else
            {
                throw new Exception("请先初始化");
            }

            return this;
        }

        /// <summary>
        /// 启动监听线程
        /// </summary>
        /// <param name="obj"></param>
        private void ListenClientConnect(object obj)
        {
            var OnlineSockets = LinyeeWebSocketConnectionBase.OnlineSockets;

            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                //clientSocket.Send(Encoding.ASCII.GetBytes("Server Say Hello"));
                new Thread(NewThreadAccept).Start(clientSocket);

            }
        }

        /// <summary>
        /// 新线程处理新请求
        /// </summary>
        /// <param name="obj"></param>
        private async void NewThreadAccept(object obj)
        {
            //连接处理
            var clientSocket = obj as Socket;
            if (obj == null) return;

            //用户长id   
            var currentSocket = new LinyeeWebSocket(clientSocket);
            CancellationToken ct = new CancellationToken();
            var rip = (System.Net.IPEndPoint)clientSocket.RemoteEndPoint;
            //var cinfo = new LinyeeConnectionInfo()
            //{
            //    LocalPort = this.port,
            //    LocalIpAddress = this.ip,
            //    Id = Guid.NewGuid().ToString(),
            //    RemotePort = rip.Port,
            //    RemoteIpAddress = rip.Address,
            //};
            long mlid = RandomNumber.GetRndLong(4444000000, 4444999999);
            int lgnType = 0;
            long timestamp = DateTime.Now.GetTimestamp();
            var socketId = Guid.NewGuid();//连接id
            var mname = "匿名用户" + mlid;//用户名称
            //var client = new LinyeeWebSocketConnection(socketId, mlid, lgnType, mname, ct, currentSocket, cinfo);
            var client = new LinyeeWebSocketConnection(socketId, mlid, lgnType, mname, ct, currentSocket,null);
            LogService.Socket10Minute("Socket", "发生新连接", client.ToJsonString());

            //后期处理
            try
            { 
                //循环获取消息
                while (true)
                {

                    //已断开连接时
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    var res = await client.RevicedMessage();
                    if (res.Code == StatusCodeEnum.Exit)
                    {
                        LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), res.Msg, res.Data);
                        break;//退出服务
                    }

                    if (currentSocket.State != WebSocketState.Open)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.Exception(ex);
                await client.CloseMsg(ex.Message);
            }
            finally
            {
                LinyeeWebSocketConnection.TryRemove(socketId);
            }
        }
    }
}

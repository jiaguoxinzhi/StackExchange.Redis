using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WS_Core.BLL;
using WS_Core.Bodys;
using WS_Core.Bodys.Json;
using WS_Core.Consts;
using WS_Core.Enums;

namespace WS_Server.Servers
{
    /// <summary>
    /// 客户端消息
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public static class LinyeeWebSocketConnectionBase_Client
    {
        #region 统计信息
        /// <summary>
        /// 获取指定类别总数
        /// </summary>
        /// <param name="client"></param>
        /// <param name="TypeCode"></param>
        /// <returns></returns>
        public static long GetCount(this LinyeeWebSocketConnectionBase client,int TypeCode=0)
        {
            return LinyeeWebSocketConnectionBase.OnlineSockets.Values.Where(p => p.TypeCode == TypeCode).LongCount();
        }
        #endregion

        #region "获取消息"
        /// <summary>
        /// 读出数据
        /// </summary>
        /// <returns></returns>
        public static async Task<ExecuteResult<string>> RevicedMessage(this LinyeeWebSocketConnectionBase client)
        {
            ExecuteResult<string> result = new ExecuteResult<string>();
            if (!client.InOnlie())
            {
                await client.CloseMsg("已被移除服务");
                return result.SetFail("已被移除服务").SetCode( StatusCodeEnum.Exit) ;
            }

            string revmsg = "";
            //读出数据
            var buffer = new ArraySegment<byte>(new byte[1024 * 1024 * 1]);//1MB缓存

            using (var ms = new MemoryStream())
            {
                var CancelToken = client.CancelToken;
                var Client = client.Client;
                //获取信息 直接结束
                WebSocketReceiveResult wsresult = null;
                do
                {
                    CancelToken.ThrowIfCancellationRequested();//抛出取消异常
                    if (Client.State != WebSocketState.Open) break;

                    try
                    {
                        wsresult = await Client.ReceiveAsync(buffer, CancelToken);
                    }
                    catch (Exception ex)
                    {
                        return result.SetException(ex).SetCode(StatusCodeEnum.Exit);
                    }
                    //LogService.WebSocket10Minute("读取结果：" + result.ToJsonString());
                    ms.Write(buffer.Array, 0, wsresult.Count);
                    if (wsresult.EndOfMessage) break;
                }
                while (!wsresult.EndOfMessage);

                //信息处理
                if (wsresult != null)
                {
                    if (wsresult.MessageType == WebSocketMessageType.Close)
                    {
                        await client.CloseMsg("客户端主动关闭");
                        return result.SetFail(wsresult.CloseStatusDescription).SetCode(StatusCodeEnum.Exit);
                    }
                    else
                    if (wsresult.MessageType == WebSocketMessageType.Text)
                    {
                        //处理已收到的消息
                        await client.RevicedClientMessage(ms);
                    }
                    else
                    {
                        await client.SendMsgFail();
                    }
                }
                else
                {
                    await client.CloseMsg("因取消或异常关闭");
                    return result.SetFail("正常关闭").SetCode(StatusCodeEnum.Exit);
                }
            }

            return result.SetOk("收到客户端消息",revmsg);
        }

        /// <summary>
        /// 处理已收到客户端消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ms"></param>
        /// <returns></returns>
        private static async Task RevicedClientMessage(this LinyeeWebSocketConnectionBase client, MemoryStream ms)
        {
            ////代理数据
            //if (!client.Agent.Connected)
            //{
            //    client.Agent.Connect("127.0.0.1", 6379);
            //}

            ////await client.SendRedisOK();
            ////return;

            ////收到客户端数据
            //ms.Seek(0, SeekOrigin.Begin);
            //var csr = new StreamReader(ms, Encoding.UTF8);
            //var crevmsg = await csr.ReadToEndAsync();
            ////Console.WriteLine("收到客户端数据：{0}", crevmsg);
            ////转发给服务端
            //ms.Seek(0, SeekOrigin.Begin);
            //var sbuf = ms.ReadBytes();            
            //int slen = 0;
            //slen = client.Agent.Send(sbuf);

            //LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), slen + "收到客户端数据：" + crevmsg.Replace("\r\n", "\\r\\n"));
            //LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), slen + "转发客户端数据：" + sbuf.ToHexString());
            ////接收服务端数据
            //var buf = new byte[65536];
            //var bufLen = 0;
            //var task1 = Task.Run(() =>
            //{
            //    bufLen = client.Agent.Receive(buf, SocketFlags.None);
            //});
            //var task2 = Task.Delay(5000);
            //var task = await Task.WhenAny(task1, task2);
            //if (task == task2)
            //{
            //    LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), slen + "获取服务端信息超时");
            //    await client.SendRedisOK();
            //}
            //else
            //{
            //    //var bb = client.Agent.ReceiveAsync(new System.Net.Sockets.SocketAsyncEventArgs() {
            //    //});
            //    var databuf = buf.Take(bufLen).ToArray();
            //    var datastr = Encoding.UTF8.GetString(databuf);

            //    //转发给客户端
            //    //Console.WriteLine("收到服务端数据：{0}", datastr);
            //    CancellationToken ct = new CancellationToken();
            //    await client.Client.SendAsync(databuf, WebSocketMessageType.Text, true, ct);
            //    LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "收到服务端数据：" + datastr.Replace("\r\n", "\\r\\n"));
            //}
            //return;

            client.LastMsgTime = DateTime.Now;
            ms.Seek(0, SeekOrigin.Begin);
            var sr = new StreamReader(ms, Encoding.UTF8);
            ms.Seek(0, SeekOrigin.Begin);
            //var sr = new StreamReader(ms, Encoding.GetEncoding("GB2312"));
            var revmsg = await sr.ReadToEndAsync();
            LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "收到消息：" + ms.GetBuffer().ToHexString());
            LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "收到消息："+ revmsg.Replace("\r\n","\\r\\n"));
            if (string.IsNullOrEmpty(revmsg))
            {
                await client.SendMsgOK();
                return;
            }
            var startchar = revmsg.ElementAt(0);
            var endchar = revmsg.ElementAt(revmsg.Length - 1);

            //回应消息 不用处理
            if (revmsg.Length == 2 && revmsg == "OK")
            {

            }
            //string redis 命令
            else if (startchar == '*' && endchar == '\n')
            {
                ms.Seek(0, SeekOrigin.Begin);
                var br = new BinaryReader(ms,ASCIIEncoding.ASCII);
                //var br = new BinaryReader(ms, Encoding.Default);
                LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "Redsi消息...");
                try
                {
                    List<REDISClientCommand> list = new List<REDISClientCommand>();
                    //循环获取命令
                    while(!(br.BaseStream.Position < 0 || br.BaseStream.Position >= br.BaseStream.Length))
                    {
                        //new Thread(async() =>
                        //{
                        try
                        {
                            var rdcmd = new REDISClientCommand(br);
                            list.Add(rdcmd);
                            await client.RunCommand(rdcmd);
                        }
                        catch (Exception ex)
                        {
                            LogService.Exception(ex);
                            await client.SendRedisFAIL(ex.Message);
                            break;
                        }
                        //}).Start();//多线程处理命令
                    }
                    //await client.RunCommand(list);
                }
                catch (Exception ex)
                {
                    LogService.Exception(ex);
                    await client.SendRedisFAIL(ex.Message);
                }
            }

            //json mq 自定义协议
            else  if (startchar == '{' && endchar == '}' || startchar == '[' && endchar == ']')
            {
                LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "Json消息...");
                MQProtocol mqmsg = JsonConvert.DeserializeObject<MQProtocol>(revmsg);
                await client.RunCommand(mqmsg, revmsg);
            }

            else
            {
                await client.SendMsgOK();
            }
        }

        /// <summary>
        /// 设置消息
        /// 模拟收到的消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="revmsg"></param>
        /// <returns></returns>
        public static async Task<string> RevicedMessage(this LinyeeWebSocketConnectionBase client, string revmsg)
        {
            LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "设置消息：" + revmsg);
            client.LastMsgTime = DateTime.Now;

            //处理已收到的消息
            await client.RevicedClientMessage(new MemoryStream(Encoding.UTF8.GetBytes(revmsg)));
            return revmsg;
        }



        #endregion

        #region "处理客户端消息"

        /// <summary>
        /// 客户端消息处理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="mqmsg"></param>
        /// <param name="revmsg"></param>
        private static async Task RunCommand(this LinyeeWebSocketConnectionBase client, MQProtocol mqmsg, string revmsg)
        {
            switch (mqmsg.Action)
            {
                case MQProtocolActionType.Response:
                    await client.RunResponse(mqmsg, revmsg);
                    break;
                case MQProtocolActionType.Request:
                    await client.RunRequest(mqmsg, revmsg);
                    break;
                case MQProtocolActionType.Redis:
                    await client.RunRedis(mqmsg, revmsg);
                    break;
                default:
                    await client.SendMsgOK();//发送一个成功标志
                    break;
            }
        }

        /// <summary>
        /// 检测登录
        /// </summary>
        /// <param name="client"></param>
        /// <param name="qinfo"></param>
        public static async Task<bool> CheckLoginRequest(this LinyeeWebSocketConnectionBase client, LoginRequest qinfo)
        {
            var sign = qinfo.GetSign();
            if (sign != qinfo.sign)
            {
                await client.CloseMsg("签名异常");
                return false;
            }

            if ((DateTime.Now - qinfo.CreateTime).TotalMinutes >= 10)
            {
                await client.CloseMsg(qinfo.CreateTime.ToString("HH:mm:ss.ffffff") + " 时间误差过大，拒绝登录");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检测登录
        /// </summary>
        /// <param name="client"></param>
        /// <param name="qinfo"></param>
        /// <param name="comp">比较的源</param>
        public static async Task<bool> CheckLoginRequest(this LinyeeWebSocketConnectionBase client, LoginRequest qinfo, LinyeeWebSocketConnectionBase comp)
        {
            var b = await client.CheckLoginRequest(qinfo);
            if (!b) return b;

            if(comp==null || comp.loginAccount!= qinfo.loginAccount || comp.loginPassword!= qinfo.loginPassword)
            {
                await client.CloseMsg("账号或密码不匹配");
                return false;
            }

            return true;
        }


        /// <summary>
        /// 登录
        /// 相同longid+主type时 会踢掉之前登录
        /// </summary>
        /// <param name="client"></param>
        /// <param name="RequestId"></param>
        /// <param name="body"></param>
        /// <param name="func"></param>
        public static async Task< ExecuteResult<bool>> Login(this LinyeeWebSocketConnectionBase client, LoginRequest body, string RequestId, Func<string,string,int, Task<ExecuteResult<MQMemberLogonInfo>>> func)
        {
            ExecuteResult<bool> result = new ExecuteResult<bool>();
            //检测登录
            var res = await func(body.loginAccount, body.DePassword,body.loginType);
            if (!res.IsOk)
            {
                await client.SendMsg(await new MQProtocol()
                {
                    Action = MQProtocolActionType.Response,
                    Command = MQProtocolCommandType.Login,
                    Data = new MQReSponseBase()
                    {
                        RequestId = RequestId,
                        StatusCode = res.Code,
                        StatusDetails = res.Msg,
                        ContentType = "application/json",
                        Content =await new MQMemberLogonInfo {
                        }.ToJsonStringAsync(),
                    }
                }.ToJsonStringAsync());

                await client.CloseMsg("登录失败");
                return result.SetFail("登录失败");
            }
            else
            {
                client.LongId = res.Data.MemberId;
                client.Id = (int)client.LongId;
                client.Authed = true;
                client.loginAccount = body.loginAccount;
                client.loginPassword = body.loginPassword;
                client.Name = res.Data.MemberRicardName;

                await client.SendMsg(await new MQProtocol()
                {
                    Action = MQProtocolActionType.Response,
                    Command = MQProtocolCommandType.Login,
                    Data = new MQReSponseBase()
                    {
                        RequestId = RequestId,
                        StatusCode = StatusCodeEnum.OK,
                        ContentType = "application/json",
                        Content = await res.Data.ToJsonStringAsync(),
                    }
                }.ToJsonStringAsync());
                return result.SetOk("登录成功");
            }

        }

        /// <summary>
        /// 客户端请求消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="mqmsg"></param>
        /// <param name="revmsg"></param>
        /// <returns></returns>
        private static async Task RunRequest(this LinyeeWebSocketConnectionBase client, MQProtocol mqmsg, string revmsg)
        {
            var mqQuest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase>>(revmsg);

            switch (mqmsg.Command)
            {
                case MQProtocolCommandType.Login:
                    {
                        var mqquest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase<LoginRequest>>>(revmsg);
                        LogService.AnyLog("ReQuestSponse", "APP端登录参数：", revmsg);
                        var body = mqquest.Data.Body;

                        await client.Login(body, mqquest.Data.RequestId, client.MemberBll.CheckLoginAsync);
                    }
                    break;
                default:
                    {
                        await client.SendMsg(await new MQReSponseBase()
                        {
                            RequestId = mqQuest.Data.RequestId,
                            StatusCode = StatusCodeEnum.Not_Found,
                            ContentType = "application/json",
                            Content = null,
                        }.ToJsonStringAsync());
                    }
                    break;
            }
        }

        /// <summary>
        /// 客户端响应消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="mqmsg"></param>
        /// <param name="revmsg"></param>
        /// <returns></returns>
        private static async Task RunResponse(this LinyeeWebSocketConnectionBase client, MQProtocol mqmsg, string revmsg)
        {
            var mqSponse = JsonConvert.DeserializeObject<MQProtocol<MQReSponseBase>>(revmsg);

            switch (mqmsg.Command)
            {
                case MQProtocolCommandType.GetPayUrl:
                    {
                        try
                        {
                            var mqsponse = JsonConvert.DeserializeObject<MQProtocol<MQReSponseBase<PayResponse>>>(revmsg);
                            client.ResponseMsg.AddOrUpdate(mqsponse.Data.RequestId, mqsponse, (key, value) => mqsponse);
                            LogService.AnyLog("ReQuestSponse", "=APP端响应参数：", revmsg);
                        }
                        catch (Exception ex)
                        {
                            LogService.Exception(ex);
                            await client.SendMsg(await new MQProtocol<MQReSponseBase<PayResponse>>()
                            {
                                Action = mqmsg.Action,
                                Command = mqmsg.Command,
                                Data = new MQReSponseBase<PayResponse>()
                                {
                                    RequestId = mqSponse.Data.RequestId,
                                    StatusCode = StatusCodeEnum.FAIL,
                                    ContentType = "application/json",
                                    Body = new PayResponse()
                                    {
                                    },
                                }
                            }.ToJsonStringAsync());
                        }
                    }
                    break;
                default:
                    {
                        await client.SendMsg(await new MQProtocol()
                        {
                            Action = mqmsg.Action,
                            Command = mqmsg.Command,
                            Data = new MQReSponseBase()
                            {
                                RequestId = mqSponse.Data.RequestId,
                                StatusCode = StatusCodeEnum.Not_Found,
                                ContentType = "application/json",
                                Content = null,
                            }
                        }.ToJsonStringAsync());
                    }
                    break;
            }
        }
        #endregion
    }
}

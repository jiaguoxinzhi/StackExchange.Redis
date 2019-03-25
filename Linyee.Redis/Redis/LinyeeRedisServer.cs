using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WS_Core.Bodys;
using WS_Core.Bodys.Redis;
using WS_Core.Consts;
using WS_Core.Enums;
using WS_Core.Tools;
using WS_Core.Utils;
using WS_Server.Servers;
using WS_Server.SocketServers;

namespace WS_Server.Redis
{
    /// <summary>
    /// redis服务
    /// </summary>
    public class LinyeeRedisServer
    {
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        public enum ShutdownReason
        {
            /// <summary>
            /// 
            /// </summary>
            ServerDisposed,
            /// <summary>
            /// 
            /// </summary>
            ClientInitiated,
        }
        //private readonly List<RedisClient> _clients = new List<RedisClient>();
        private readonly TextWriter _output;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        public LinyeeRedisServer(TextWriter output = null)
        {
            _output = output;
            //_commands = BuildCommands(this);
        }

        /// <summary>
        /// 获取文本
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        unsafe string GetString(ref ReadOnlySequence<byte> buffer)
        {
            //if (buffer.Length < 1) return null;
            fixed (byte* ptr = &MemoryMarshal.GetReference(buffer.First.Span))
            {
                var str = Encoding.UTF8.GetString(ptr, (int)buffer.Length);
                buffer = buffer.Slice(buffer.Length);
                return str;
            }
        }

        /// <summary>
        /// 调用入口
        /// </summary>
        /// <param name="transport"></param>
        /// <returns></returns>
        public async Task RunClientAsync(ConnectionContext connection )
        {
            //写入到管道
            void write(PipeWriter output, string str)
            {
                var buf = Encoding.UTF8.GetBytes(str);
                var blen = buf.Length;
                var ospan = output.GetSpan(buf.Length);
                ospan = new Span<byte>(buf);
                output.Advance(blen);
            }

            IDuplexPipe pipe = connection.Transport;
            Exception fault = null;
            Guid gid = Guid.NewGuid();

            //用户长id   
            var currentSocket = new LinyeeWebSocket(pipe);
            CancellationToken ct = new CancellationToken();
            var cinfo = new LinyeeConnectionInfo()
            {
                RemoteIpAddress=IPAddress.Any,
                RemotePort=0,
                LocalIpAddress= IPAddress.Any,
                LocalPort=0,
            };
            long mlid = RandomNumber.GetRndLong(4444000000, 4444999999);
            int lgnType = 0;
            long timestamp = DateTime.Now.GetTimestamp();
            var socketId = Guid.NewGuid();//连接id
            var mname = "匿名用户" + mlid;//用户名称
            var client = new LinyeeWebSocketConnection(socketId, mlid, lgnType, mname, ct, currentSocket, cinfo);
            LogService.Socket10Minute("Socket", "发生新连接", client.ToJsonString());

            while (!client.Closed)
            {
                var readResult = await pipe.Input.ReadAsync().ConfigureAwait(false);
                var buffer = readResult.Buffer;
                var str = GetString(ref buffer);
                var output = pipe.Output;
                Console.WriteLine("收到消息:{0}", str);

                write(output, "+OK");//输出
                write(output, "\r\n");//输出
                await output.FlushAsync();

                bool makingProgress = false;
                if(buffer.IsEmpty) makingProgress = true;
                pipe.Input.AdvanceTo(buffer.Start, buffer.End);//循环接收

                if (!makingProgress && readResult.IsCompleted)
                { 
                    // nothing to do, and nothing more will be arriving
                    break;
                }
            }

        }

    }

    public static class LinyeeRedisServerEx
    {
        #region "redis协议"
        #region "发送消息"
        /// <summary>
        /// 发送一个Redis成功数据
        /// </summary>
        public static async Task SendRedisOK(this LinyeeWebSocketConnectionBase client, string msg = "OK")
        {
            await client.SendMsg(string.Format("+{0}\r\n", msg));
        }
        /// <summary>
        /// 发送一个Redis失败数据
        /// </summary>
        public static async Task SendRedisFAIL(this LinyeeWebSocketConnectionBase client, string msg = "")
        {
            await client.SendMsg(string.Format("-FAIL {0}\r\n", msg));
        }
        /// <summary>
        /// 发送一个Redis错误数据
        /// </summary>
        public static async Task SendRedisERR(this LinyeeWebSocketConnectionBase client, string msg = "")
        {
            await client.SendMsg(string.Format("-ERR {0}\r\n", msg));
        }
        /// <summary>
        /// 发送一个Redis数据数据
        /// </summary>
        public static async Task SendRedisInteger(this LinyeeWebSocketConnectionBase client, long count)
        {
            await client.SendMsg(string.Format(":{0}\r\n", count));
        }
        /// <summary>
        /// 发送一个Redis数据数据
        /// </summary>
        public static async Task SendRedisInteger(this LinyeeWebSocketConnectionBase client, object value)
        {
            await client.SendMsg(string.Format(":{0}\r\n", value.ToString()));
        }
        /// <summary>
        /// 发送一个Redis簇数据
        /// </summary>
        public static async Task SendRedisBulk(this LinyeeWebSocketConnectionBase client, string msg = null)
        {
            if (msg == null)
            {
                await client.SendMsg("$-1\r\n");
            }
            else
            {
                await client.SendMsg(RedisBulkBuilder(msg));
            }
        }

        /// <summary>
        /// 发送一个Redis成功消息
        /// </summary>
        public static async Task SendRedisOKBulk(this LinyeeWebSocketConnectionBase client, string msg, string okmsg = "OK")
        {
            await client.SendMsg(string.Format("+{0}\r\n{1}", okmsg, RedisBulkBuilder(msg)));
        }

        /// <summary>
        /// 发送一个Redis成功消息组
        /// </summary>
        public static async Task SendRedisOKArray(this LinyeeWebSocketConnectionBase client, params string[] args)
        {
            await client.SendMsg(string.Format("+OK\r\n{0}", RedisArrayBuilder(args)));
        }


        /// <summary>
        /// 发送一个Redis数据组
        /// </summary>
        public static async Task SendRedisArray(this LinyeeWebSocketConnectionBase client, params string[] args)
        {
            if (args != null || args.LongLength > 0)
            {
                await client.SendMsg(RedisArrayBuilder(args));
            }
        }

        /// <summary>
        /// 发送一个Redis数据组
        /// </summary>
        public static async Task SendRedisArrayCount(this LinyeeWebSocketConnectionBase client, params string[] args)
        {
            if (args != null || args.LongLength > 0)
            {
                await client.SendMsg(RedisArrayBuilderCount(args));
            }
        }

        /// <summary>
        /// 组装Redis组数据
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string RedisArrayBuilderCount(params string[] args)
        {
            if (args != null || args.LongLength > 0)
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("*");
                sbd.Append(args.LongLength + 1);
                sbd.AppendLine();

                foreach (var msg in args)
                {
                    sbd.Append(RedisBulkBuilder(msg));
                }

                sbd.Append(":");
                sbd.Append(args.LongLength);
                sbd.AppendLine();
                return sbd.ToString();
            }

            return "";
        }

        /// <summary>
        /// 组装Redis组数据
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string RedisArrayBuilder(params string[] args)
        {
            if (args != null || args.LongLength > 0)
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("*");
                sbd.Append(args.LongLength);
                sbd.AppendLine();

                foreach (var msg in args)
                {
                    sbd.Append(RedisBulkBuilder(msg));
                }
                return sbd.ToString();
            }

            return "";
        }

        /// <summary>
        /// 组装Redis簇数据
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string RedisBulkBuilder(string msg)
        {
            if (msg == null)
            {
                return "$-1\r\n";
            }
            return string.Format("${0}\r\n{1}\r\n", msg.Length, msg);
        }

        /// <summary>
        /// 组装Redis簇数据
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string RedisErrorBuilder(string msg)
        {
            return string.Format("-ERR {0}\r\n", msg);
        }

        /// <summary>
        /// 发送一个Redis不被支持的动作数据
        /// </summary>
        public static async Task SendRedisNonSupportAction(this LinyeeWebSocketConnectionBase client, char c)
        {
            await client.SendMsg(string.Format("-Non Support Action '{0}'\r\n", c));
        }

        /// <summary>
        /// 发送一个Redis未知命令数据
        /// </summary>
        public static async Task SendRedisUnkownCommand(this LinyeeWebSocketConnectionBase client, string cmd)
        {
            await client.SendMsg(string.Format("-Non Unkown Command '{0}'\r\n", cmd));
        }
        #endregion


        #region "Redis 命令"
        /// <summary>
        /// 运行Redis
        /// </summary>
        /// <param name="rdmsg"></param>
        /// <returns></returns>
        public static async Task RunCommand(this LinyeeWebSocketConnectionBase client, List<REDISClientCommand> rdmsg)
        {
            List<string> list = new List<string>();
            foreach (var cmd in rdmsg)
            {
                var res = RunCommandResult(client, cmd).Result;
                list.Add(res.Data);
            }
            await client.SendMsg(string.Join("", list));
        }

        private static async Task<ExecuteResult<string>> RunCommandResult(LinyeeWebSocketConnectionBase client, REDISClientCommand rediscmd)
        {
            ExecuteResult<string> result = new ExecuteResult<string>();
            var bodys = rediscmd.body;
            var cmd = bodys[0];
            var args = bodys.Skip(1).ToArray();
            LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "当前命令", cmd.body);
            switch ((RedisCommand)Enum.Parse(typeof(RedisCommand), cmd.body.ToUpper()))
            {
                #region "原生客户端"
                //客户端
                case RedisCommand.CLIENT:
                    //goto case RedisCommand.REDISAGENT;
                    {
                        if (args.Length < 2)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }

                        var subcmd = args[0].body;
                        switch ((RedisSubCommand)Enum.Parse(typeof(RedisSubCommand), subcmd.ToUpper()))
                        {
                            case RedisSubCommand.SETNAME:
                                client.Name = args[1].body;
                                return result.SetData(REDISProtocol.OKString);
                            default:
                                break;
                        }
                    }
                    break;
                //配置
                case RedisCommand.CONFIG:
                    //goto case RedisCommand.REDISAGENT;
                    {
                        if (args.Length < 2)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }

                        var subcmd = args[0].body;//子命令
                        var key = args[1].body;//操作 对象名
                        switch ((RedisSubCommand)Enum.Parse(typeof(RedisSubCommand), subcmd.ToUpper()))
                        {
                            case RedisSubCommand.GET:
                                var val = RedisConfig.Default.GetValue(key);
                                if (val.IsOk)
                                {
                                    return result.SetData(RedisArrayBuilder(key, val.Data?.ToString()));
                                }
                                else
                                {
                                    return result.SetData(RedisErrorBuilder(val.Msg));
                                }
                            default:
                                break;
                        }
                    }
                    break;
                //信息
                #region 信息
                case RedisCommand.INFO:
                    //goto case RedisCommand.REDISAGENT;
                    if (args.Length < 1)
                    {
                        return result.SetData(RedisBulkBuilder(RedisInfoV1_0.Default.All));
                    }
                    else
                    {
                        var key = args[0].body;
                        var res = RedisInfoV1_0.Default.GetValue(key.ToLower());
                        if (res.IsOk) return result.SetData(RedisBulkBuilder(res.Data));
                        else return result.SetData(RedisErrorBuilder(res.Msg));
                    }
                    break;
                //信息
                case RedisCommand.CLUSTER:
                    //goto case RedisCommand.REDISAGENT;
                    {
                        if (args.Length < 1)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }

                        var key = args[0].body;//操作 对象名
                        return result.SetData(RedisErrorBuilder("This instance has cluster support disabled"));
                    }
                    break;
                //服务端上显示信息
                case RedisCommand.ECHO:
                    //goto case RedisCommand.REDISAGENT;
                    {
                        if (args.Length < 1)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }
                        var msg = args[0].body;//子命令
                        LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "回显内容", msg);
                        return result.SetData(RedisBulkBuilder(msg));
                    }
                    break;
                #endregion

                #endregion

                #region "缓存"
                case RedisCommand.SET:
                    {
                        var cres = client.CheckRedisKeyResult(args[0].body).Result;
                        if (!cres.IsOk)
                        {
                            return result.SetData(RedisErrorBuilder(cres.Data));
                        }

                        var RedisMulti = LinyeeWebSocketConnectionBase.RedisMulti;
                        string msg = "OK";
                        //如果内存已有80% 或已有1000次，则先执行现有事务
                        if (CpuCount_Helper.GetCurMemUseRate() > REDISProtocol.RedisMaxMemUseRate || client.RedisMultiCount() >= REDISProtocol.RedisOperMaxCount)
                        {
                            await client.RedisMultiEXEC(false);
                            client.IsRedisMulti = true;
                            msg += " 事务已达标，自动执行完成";
                        }

                        if (args.Length < 2)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }

                        long sec = 0;
                        if (args.Length >= 3) long.TryParse(args[2].body, out sec);

                        if (client.IsRedisMulti)
                        {
                            LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "进入三个参数事务");
                            await client.EnqueueRedisMulti((c) => c.RedisSet(args[0].body, args[1].body, sec));
                            return result.SetData(REDISProtocol.OKString);
                        }
                        else
                        {
                            var res = client.RedisSet(args[0].body, args[1].body, sec);
                            if (res != null) return result.SetData(REDISProtocol.OKString);
                            else return result.SetData(RedisErrorBuilder(msg));
                        }
                    }
                    break;
                case RedisCommand.GET:
                    {
                        var res = client.RedisGet(args[0].body);
                        if (res != null) return result.SetData(RedisBulkBuilder(res.GetValue()));
                        else return result.SetData(RedisBulkBuilder(null));
                    }

                #endregion

                #region "服务端"
                case RedisCommand.PING:
                    {
                        return result.SetData("+PONG\r\n");
                    }
                case RedisCommand.SAVE:
                    {
                        var res = await client.RedisSave();
                        if (res.IsOk)
                        {
                            return result.SetData(REDISProtocol.OKString);
                        }
                        else
                        {
                            return result.SetData(RedisErrorBuilder(res.Msg));
                        }
                    }
                case RedisCommand.QUIT:
                    {
                        await client.CloseMsg("QUIT命令");
                        return result.SetData(REDISProtocol.OKString);
                    }
                #endregion

                #region "队列缓存 List"
                case RedisCommand.ENQ:
                case RedisCommand.LPUSH:
                    {
                        var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
                        if (args.Length < 2)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }
                        var key = args[0].body;
                        var val = args[1].body;
                        Queue<string> que = new Queue<string>();
                        if (!RedisQueue.ContainsKey(key)) RedisQueue.TryAdd(key, que);
                        else que = RedisQueue[key];
                        que.Enqueue(val);
                        return result.SetData(REDISProtocol.OKString);
                    }
                case RedisCommand.DEQ:
                case RedisCommand.LPOP:
                    {
                        var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
                        if (args.Length < 1)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }
                        var key = args[0].body;
                        Queue<string> que = new Queue<string>();
                        if (!RedisQueue.ContainsKey(key)) RedisQueue.TryAdd(key, que);
                        else que = RedisQueue[key];

                        if (que.Count < 1)
                        {
                            return result.SetData(RedisBulkBuilder(null));
                        }
                        var val = que.Dequeue();
                        return result.SetData(RedisBulkBuilder(val));
                    }
                case RedisCommand.PEEKQ:
                    {
                        var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
                        if (args.Length < 1)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }
                        var key = args[0].body;
                        Queue<string> que = new Queue<string>();
                        if (!RedisQueue.ContainsKey(key)) RedisQueue.TryAdd(key, que);
                        else que = RedisQueue[key];

                        if (que.Count < 1)
                        {
                            return result.SetData(RedisBulkBuilder(null));
                        }
                        var val = que.Peek();
                        return result.SetData(RedisBulkBuilder(val));
                    }
                case RedisCommand.LLEN:
                case RedisCommand.LENQ:
                    {
                        var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
                        if (args.Length < 1)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }
                        var key = args[0].body;
                        Queue<string> que = new Queue<string>();
                        if (!RedisQueue.ContainsKey(key)) RedisQueue.TryAdd(key, que);
                        else que = RedisQueue[key];

                        var val = que.Count;
                        return result.SetData(RedisBulkBuilder(val.ToString()));
                    }
                #endregion

                #region "队列消息 主题"
                case RedisCommand.PSUBSCRIBE://泛关注
                    {
                        var RedisTopic = LinyeeWebSocketConnectionBase.RedisTopic;
                        var OnlineSockets = LinyeeWebSocketConnectionBase.OnlineSockets;
                        if (args.Length < 1)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }
                        var topics = args.Select(p => p.body);

                        List<string> lsb = new List<string>();
                        foreach (var topic in topics)
                        {
                            //检测
                            //兼容redis规范不启用
                            //if (topic?.StartsWith("/") != true)
                            //{
                            //    lsb.Add(string.Format("{0} 主题必须以/开头", topic));
                            //    continue;
                            //}

                            if (topic.IndexOf("*") >= 0 && topic.IndexOf("#") >= 0)
                            {
                                lsb.Add(string.Format("{0} 主题不能#*同时存在", topic));
                                continue;
                            }

                            //泛关注 只保留最后一次
                            if (topic.IndexOf("#") >= 0)
                            {
                                client.Topic = "^" + topic.Replace("#", "[\\w\\/]*") + "$";
                                //更新到其它端
                                OnlineSockets.Values.Where(p => p.LongId == client.LongId && p.ClientId != client.ClientId)
                                    .Select(p => {
                                        p.Topic = client.Topic;
                                        return p;
                                    }).ToList();
                                continue;
                            }

                            //现有正则关注
                            var regtopic = topic.Replace("*", "[\\w]+");
                            Regex topicrgx = new Regex(regtopic, RegexOptions.Compiled);

                            var tpcs = RedisTopic.Keys.Where(p => topicrgx.IsMatch(p));
                            foreach (var tpc in tpcs)
                            {
                                var item = RedisTopic[tpc];//获取主题
                                if (item.TypeCode >= client.TypeCode && item.TypeCode < 1000000000000L)
                                {
                                    item.SubIds.Add(client.LongId);//添加到关注名单
                                    if (item.TypeCode < 1000000000000 && OnlineSockets.Count > 0 && !OnlineSockets.Values.Any(p => p.LongId == item.LongId)) item.LongId = OnlineSockets.Values.FirstOrDefault().LongId;//修改房主
                                }
                                else
                                {
                                    lsb.Add(string.Format("{0} 此主题您无权关注", tpc));
                                }
                            }
                        }

                        if (lsb.Count > 0)
                        {
                            return result.SetData(RedisArrayBuilderCount(lsb.ToArray()));
                        }
                        else
                        {
                            return result.SetData(REDISProtocol.OKString);
                        }

                    }
                case RedisCommand.PUNSUBSCRIBE://泛取消关注
                    {
                        client.Topic = null;
                        return result.SetData(REDISProtocol.OKString);
                    }
                case RedisCommand.SUBSCRIBE://关注
                    {
                        var RedisTopic = LinyeeWebSocketConnectionBase.RedisTopic;
                        var OnlineSockets = LinyeeWebSocketConnectionBase.OnlineSockets;
                        if (args.Length < 1)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }
                        var topics = args.Select(p => p.body);

                        List<string> lsb = new List<string>();
                        foreach (var topic in topics)
                        {
                            //检测//|| topic?.EndsWith("/") != true
                            //兼容redis规范不启用
                            //if (topic?.StartsWith("/") != true )
                            //{
                            //    lsb.Add(string.Format("{0} 主题必须以/开头", topic));
                            //    continue;
                            //}

                            if (topic.IndexOf("*") >= 0 || topic.IndexOf("#") >= 0)
                            {
                                lsb.Add(string.Format("{0} 主题带#*，请用PSUBSCRIBE命令", topic));
                                continue;
                            }

                            var res = await client.AddTopic(topic);
                            if (res.IsOk) lsb.Add(topic);//返回关注成功能主题
                        }

                        if (lsb.Count > 0)
                        {
                            return result.SetData(RedisArrayBuilderCount(lsb.ToArray()));
                        }
                        else
                        {
                            return result.SetData(REDISProtocol.OKString);
                        }
                    }
                case RedisCommand.UNSUBSCRIBE: //取消关注
                    {
                        var RedisTopic = LinyeeWebSocketConnectionBase.RedisTopic;
                        if (args.Length < 1)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }
                        var topics = args.Select(p => p.body);
                        foreach (var topic in topics)
                        {
                            if (RedisTopic.ContainsKey(topic))
                            {
                                var item = RedisTopic[topic];//获取主题
                                if (item.SubIds.Contains(client.LongId))
                                {
                                    var res = RedisTopic.RemoveSub(item, client);
                                }
                            }
                        }
                        return result.SetData(REDISProtocol.OKString);
                    }
                case RedisCommand.PUBLISH://发布 主题消息
                    {
                        var RedisQueueMessage = LinyeeWebSocketConnectionBase.RedisQueueMessage;
                        if (args.Length < 2)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }

                        var RedisTopic = LinyeeWebSocketConnectionBase.RedisTopic;
                        var topic = args[0].body;
                        var msg = string.Join("\t", args.Skip(1).Select(p => p.body));

                        //主题
                        var tism = client.TopicRegex?.IsMatch(topic);
                        //Console.WriteLine("正则数据：{0} 检测结果：{1}", client.TopicRegex, tism);
                        var ism = tism == true;
                        if (!RedisTopic.ContainsKey(topic) && !ism)
                        {
                            return result.SetData(RedisErrorBuilder(string.Format("不存在主题{0}，请先关注或创建", topic)));
                        }
                        else if (!RedisTopic.ContainsKey(topic) && ism)
                        {
                            var res = await client.AddTopic(topic);
                            if (!res.IsOk)
                            {
                                return result.SetData(RedisErrorBuilder(string.Format("主题{0}，添加失败", topic)));
                            }
                        }

                        //队列消息
                        Queue<QueueMessage> item = null;
                        if (!RedisQueueMessage.ContainsKey(topic))
                        {
                            item = new Queue<QueueMessage>();
                            RedisQueueMessage.TryAdd(topic, item);
                        }
                        else
                        {
                            item = RedisQueueMessage[topic];
                        }

                        var quemsg = new QueueMessage()
                        {
                            p = topic,
                            u = client.LongId,
                            m = msg,
                            t = DateTime.Now.GetTimestamp(),
                            d = 0L,
                        };
                        item.Enqueue(quemsg);
                        var tpitem = RedisTopic[topic];

                        //发送队列消息的线程
                        new Thread(async () => {
                            var OnlineSockets = LinyeeWebSocketConnectionBase.OnlineSockets;
                            while (item.Count > 0)
                            {
                                var qmsg = item.Dequeue();
                                var quejson = qmsg.ToJsonString();//&& p.LongId != client.LongId//加上这个自己发的不接收
                                var qids = OnlineSockets.Values
                                .Where(p => (p.TypeCode / 100) % 100 == 1)//只向事件端发送数据
                                .Where(p => (quemsg.d == 0 && (tpitem.SubIds.Contains(p.LongId) || p.TopicRegex?.IsMatch(qmsg.p) == true)) || (quemsg.d > 0 && quemsg.d == p.LongId)).ToList();
                                foreach (var id in qids)
                                {
                                    try
                                    {
                                        await id.SendRedisArray(RedisCommand.PUBLISH.ToString(), qmsg.p, quejson);
                                    }
                                    catch (Exception ex)
                                    {
                                        var exmsg = LogService.Exception(ex);
                                        LogService.AnyLog("Topic", id.LongId.ToString(), topic, exmsg);
                                    }
                                }
                            }
                        }).Start();

                        return result.SetData(REDISProtocol.OKString);
                    }
                case RedisCommand.PUBSUB://获取主题信息
                    {
                        var RedisTopic = LinyeeWebSocketConnectionBase.RedisTopic;
                        if (args.Length < 1)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }

                        var topic = args[0].body;
                        if (RedisTopic.ContainsKey(topic))
                        {
                            var item = RedisTopic[topic];//获取主题
                            if (item.LongId == client.LongId)
                            {
                                return result.SetData(REDISProtocol.OKString);
                            }
                            else
                            {
                                return result.SetData(REDISProtocol.OKString);
                            }
                        }
                        else
                        {
                            return result.SetData(RedisErrorBuilder(string.Format("不存在主题{0}，请先关注或创建", topic)));
                        }
                    }
                #endregion

                default:
                    return result.SetData(RedisErrorBuilder("non super " + cmd.body));
            }
            return result.SetData(RedisErrorBuilder("error excuting " + cmd.body));
        }

        /// <summary>
        /// 运行Redis
        /// </summary>
        /// <param name="rdmsg"></param>
        /// <returns></returns>
        public static async Task RunCommand(this LinyeeWebSocketConnectionBase client, REDISClientCommand rdmsg)
        {
            switch (rdmsg.action)
            {
                case REDIS.Arrays:
                    {
                        var cmd = rdmsg.body[0];
                        var args = rdmsg.body.Skip(1).ToArray();
                        await client.RunCommand(cmd, args);
                    }
                    break;
                default:
                    await client.SendRedisNonSupportAction(rdmsg.action);//发送一个标志
                    break;
            }
        }


        /// <summary>
        /// 运行Redis
        /// </summary>
        /// <param name="rdmsg"></param>
        /// <returns></returns>
        public static async Task RunCommand(this LinyeeWebSocketConnectionBase client, REDISBulk cmd, params REDISBulk[] args)
        {
            LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "当前命令", cmd.body);
            switch ((RedisCommand)Enum.Parse(typeof(RedisCommand), cmd.body.ToUpper()))
            {
                #region "原生客户端"
                //case RedisCommand.REDISAGENT:
                //    ////代理数据
                //    //if (client.Agent.State!= WebSocketState.Open)
                //    //{
                //    //   await client.Agent.ConnectAsync("127.0.0.1",6339);
                //    //}

                //    //client.Agent.Send();
                //    break;
                //客户端
                case RedisCommand.CLIENT:
                    //goto case RedisCommand.REDISAGENT;
                    {
                        if (args.Length < 2)
                        {
                            await client.SendRedisFAIL("参数不足");
                            return;
                        }

                        var subcmd = args[0].body;
                        switch ((RedisSubCommand)Enum.Parse(typeof(RedisSubCommand), subcmd.ToUpper()))
                        {
                            case RedisSubCommand.SETNAME:
                                client.Name = args[1].body;
                                await client.SendRedisOK();
                                return;
                            default:
                                break;
                        }
                    }
                    break;
                //配置
                case RedisCommand.CONFIG:
                    //goto case RedisCommand.REDISAGENT;
                    {
                        if (args.Length < 2)
                        {
                            await client.SendRedisFAIL("参数不足");
                            return;
                        }

                        var subcmd = args[0].body;//子命令
                        var key = args[1].body;//操作 对象名
                        switch ((RedisSubCommand)Enum.Parse(typeof(RedisSubCommand), subcmd.ToUpper()))
                        {
                            case RedisSubCommand.GET:
                                var val = RedisConfig.Default.GetValue(key);
                                if (val.IsOk)
                                {
                                    await client.SendRedisArray(key, val.Data?.ToString());
                                }
                                else
                                {
                                    await client.SendRedisFAIL(val.Msg);
                                }
                                return;
                            default:
                                break;
                        }
                    }
                    break;
                //信息
                #region 信息
                case RedisCommand.INFO:
                    //goto case RedisCommand.REDISAGENT;
                    if (args.Length < 1)
                    {
                        await client.SendRedisBulk(RedisInfoV1_0.Default.All);
                    }
                    else
                    {
                        var key = args[0].body;
                        var res = RedisInfoV1_0.Default.GetValue(key.ToLower());
                        if (res.IsOk) await client.SendRedisBulk(res.Data);
                        else await client.SendRedisERR(res.Msg);
                    }
                    break;
                //信息
                case RedisCommand.CLUSTER:
                    //goto case RedisCommand.REDISAGENT;
                    {
                        if (args.Length < 1)
                        {
                            await client.SendRedisFAIL("参数不足");
                            return;
                        }

                        var key = args[0].body;//操作 对象名
                        await client.SendRedisERR("This instance has cluster support disabled");
                    }
                    break;
                //服务端上显示信息
                case RedisCommand.ECHO:
                    //goto case RedisCommand.REDISAGENT;
                    {
                        if (args.Length < 1)
                        {
                            await client.SendRedisFAIL("参数不足");
                            return;
                        }
                        var msg = args[0].body;//子命令
                        LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "回显内容", msg);
                        await client.SendRedisBulk(msg);
                        return;
                    }
                    break;
                #endregion

                #endregion

                #region "缓存"
                case RedisCommand.MULTI:
                    {
                        if (client.IsRedisMulti)
                        {
                            await client.SendRedisFAIL("先前的事务未完成");
                            return;
                        }
                        else
                        {
                            client.IsRedisMulti = true;
                            await client.SendRedisOK();
                        }
                    }
                    break;
                case RedisCommand.SET:
                    {
                        if (!await client.CheckRedisKey(args[0].body))
                        {
                            return;
                        }

                        var RedisMulti = LinyeeWebSocketConnectionBase.RedisMulti;
                        string msg = "OK";
                        //如果内存已有80% 或已有1000次，则先执行现有事务
                        if (CpuCount_Helper.GetCurMemUseRate() > REDISProtocol.RedisMaxMemUseRate || client.RedisMultiCount() >= REDISProtocol.RedisOperMaxCount)
                        {
                            await client.RedisMultiEXEC(false);
                            client.IsRedisMulti = true;
                            msg += " 事务已达标，自动执行完成";
                        }

                        if (args.Length < 2)
                        {
                            await client.SendRedisFAIL("参数个数不正确" + msg);
                            return;
                        }

                        long sec = 0;
                        if (args.Length >= 3) long.TryParse(args[2].body, out sec);

                        if (client.IsRedisMulti)
                        {
                            LogService.WebSocket10Minute(client.ClientId.ToString(), client.LongId.ToString(), "进入三个参数事务");
                            await client.EnqueueRedisMulti((c) => c.RedisSet(args[0].body, args[1].body, sec));
                            await client.SendRedisOK(msg);
                        }
                        else
                        {
                            var res = client.RedisSet(args[0].body, args[1].body, sec);
                            if (res != null) await client.SendRedisOK(msg);
                            else await client.SendRedisFAIL(msg);
                        }
                    }
                    break;
                case RedisCommand.GET:
                    {
                        var res = client.RedisGet(args[0].body);
                        if (res != null) await client.SendRedisBulk(res.GetValue());
                        else await client.SendRedisBulk();
                    }
                    break;
                case RedisCommand.EXEC:
                    {
                        await client.RedisMultiEXEC();
                    }
                    break;
                case RedisCommand.DISCARD:
                    {
                        client.IsRedisMulti = false;//清空事务
                    }
                    break;
                case RedisCommand.EXISTS:
                    {
                        await client.RedisExists(args[0].body);
                    }
                    break;

                #endregion

                #region "服务端"
                case RedisCommand.PING:
                    {
                        await client.SendRedisOK("PONG");
                    }
                    break;
                case RedisCommand.SAVE:
                    {
                        var res = await client.RedisSave();
                        if (res.IsOk)
                        {
                            await client.SendRedisOK(res.Msg + "，数据库总字节数：" + res.Data.ToGMKB());
                        }
                        else
                        {
                            await client.SendRedisFAIL(res.Msg);
                        }
                    }
                    break;
                case RedisCommand.QUIT:
                    {
                        await client.CloseMsg("QUIT命令");
                    }
                    break;
                #endregion

                #region "队列缓存 List"
                case RedisCommand.ENQ:
                case RedisCommand.LPUSH:
                    {
                        var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
                        if (args.Length < 2)
                        {
                            await client.SendRedisFAIL("参数个数不正确");
                            return;
                        }
                        var key = args[0].body;
                        var val = args[1].body;
                        Queue<string> que = new Queue<string>();
                        if (!RedisQueue.ContainsKey(key)) RedisQueue.TryAdd(key, que);
                        else que = RedisQueue[key];
                        que.Enqueue(val);
                        await client.SendRedisOK();
                        return;
                    }
                    break;
                case RedisCommand.DEQ:
                case RedisCommand.LPOP:
                    {
                        var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
                        if (args.Length < 1)
                        {
                            await client.SendRedisFAIL("参数个数不正确");
                            return;
                        }
                        var key = args[0].body;
                        Queue<string> que = new Queue<string>();
                        if (!RedisQueue.ContainsKey(key)) RedisQueue.TryAdd(key, que);
                        else que = RedisQueue[key];

                        if (que.Count < 1)
                        {
                            await client.SendRedisBulk();
                            return;
                        }
                        var val = que.Dequeue();
                        await client.SendRedisBulk(val);
                    }
                    break;
                case RedisCommand.PEEKQ:
                    {
                        var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
                        if (args.Length < 1)
                        {
                            await client.SendRedisFAIL("参数个数不正确");
                            return;
                        }
                        var key = args[0].body;
                        Queue<string> que = new Queue<string>();
                        if (!RedisQueue.ContainsKey(key)) RedisQueue.TryAdd(key, que);
                        else que = RedisQueue[key];

                        if (que.Count < 1)
                        {
                            await client.SendRedisBulk();
                            return;
                        }
                        var val = que.Peek();
                        await client.SendRedisBulk(val);
                    }
                    break;
                case RedisCommand.LLEN:
                case RedisCommand.LENQ:
                    {
                        var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
                        if (args.Length < 1)
                        {
                            await client.SendRedisFAIL("参数个数不正确");
                            return;
                        }
                        var key = args[0].body;
                        Queue<string> que = new Queue<string>();
                        if (!RedisQueue.ContainsKey(key)) RedisQueue.TryAdd(key, que);
                        else que = RedisQueue[key];

                        var val = que.Count;
                        await client.SendRedisBulk(val.ToString());
                    }
                    break;
                #endregion

                #region "队列消息 主题"
                case RedisCommand.PSUBSCRIBE://泛关注
                    {
                        var RedisTopic = LinyeeWebSocketConnectionBase.RedisTopic;
                        var OnlineSockets = LinyeeWebSocketConnectionBase.OnlineSockets;
                        if (args.Length < 1)
                        {
                            await client.SendRedisFAIL("参数个数不正确");
                            return;
                        }
                        var topics = args.Select(p => p.body);

                        List<string> lsb = new List<string>();
                        foreach (var topic in topics)
                        {
                            //检测
                            //兼容redis规范不启用
                            //if (topic?.StartsWith("/") != true)
                            //{
                            //    lsb.Add(string.Format("{0} 主题必须以/开头", topic));
                            //    continue;
                            //}

                            if (topic.IndexOf("*") >= 0 && topic.IndexOf("#") >= 0)
                            {
                                lsb.Add(string.Format("{0} 主题不能#*同时存在", topic));
                                continue;
                            }

                            //泛关注 只保留最后一次
                            if (topic.IndexOf("#") >= 0)
                            {
                                client.Topic = "^" + topic.Replace("#", "[\\w\\/]*") + "$";
                                //更新到其它端
                                OnlineSockets.Values.Where(p => p.LongId == client.LongId && p.ClientId != client.ClientId)
                                    .Select(p => {
                                        p.Topic = client.Topic;
                                        return p;
                                    }).ToList();
                                continue;
                            }

                            //现有正则关注
                            var regtopic = topic.Replace("*", "[\\w]+");
                            Regex topicrgx = new Regex(regtopic, RegexOptions.Compiled);

                            var tpcs = RedisTopic.Keys.Where(p => topicrgx.IsMatch(p));
                            foreach (var tpc in tpcs)
                            {
                                var item = RedisTopic[tpc];//获取主题
                                if (item.TypeCode >= client.TypeCode && item.TypeCode < 1000000000000L)
                                {
                                    item.SubIds.Add(client.LongId);//添加到关注名单
                                    if (item.TypeCode < 1000000000000 && OnlineSockets.Count > 0 && !OnlineSockets.Values.Any(p => p.LongId == item.LongId)) item.LongId = OnlineSockets.Values.FirstOrDefault().LongId;//修改房主
                                }
                                else
                                {
                                    lsb.Add(string.Format("{0} 此主题您无权关注", tpc));
                                }
                            }
                        }

                        if (lsb.Count > 0)
                        {
                            await client.SendRedisArrayCount(lsb.ToArray());
                        }
                        else
                        {
                            await client.SendRedisOK();
                        }

                        return;
                    }
                    break;
                case RedisCommand.PUNSUBSCRIBE://泛取消关注
                    {
                        client.Topic = null;
                        await client.SendRedisOK();
                    }
                    break;
                case RedisCommand.SUBSCRIBE://关注
                    {
                        var RedisTopic = LinyeeWebSocketConnectionBase.RedisTopic;
                        var OnlineSockets = LinyeeWebSocketConnectionBase.OnlineSockets;
                        if (args.Length < 1)
                        {
                            await client.SendRedisFAIL("参数个数不正确");
                            return;
                        }
                        var topics = args.Select(p => p.body);

                        List<string> lsb = new List<string>();
                        foreach (var topic in topics)
                        {
                            //检测//|| topic?.EndsWith("/") != true
                            //兼容redis规范不启用
                            //if (topic?.StartsWith("/") != true )
                            //{
                            //    lsb.Add(string.Format("{0} 主题必须以/开头", topic));
                            //    continue;
                            //}

                            if (topic.IndexOf("*") >= 0 || topic.IndexOf("#") >= 0)
                            {
                                lsb.Add(string.Format("{0} 主题带#*，请用PSUBSCRIBE命令", topic));
                                continue;
                            }

                            var res = await client.AddTopic(topic);
                            if (res.IsOk) lsb.Add(topic);//返回关注成功能主题
                        }

                        if (lsb.Count > 0)
                        {
                            await client.SendRedisArrayCount(lsb.ToArray());
                        }
                        else
                        {
                            await client.SendRedisOK();
                        }

                        return;
                    }
                    break;
                case RedisCommand.UNSUBSCRIBE: //取消关注
                    {
                        var RedisTopic = LinyeeWebSocketConnectionBase.RedisTopic;
                        if (args.Length < 1)
                        {
                            await client.SendRedisFAIL("参数个数不正确");
                            return;
                        }
                        var topics = args.Select(p => p.body);
                        foreach (var topic in topics)
                        {
                            if (RedisTopic.ContainsKey(topic))
                            {
                                var item = RedisTopic[topic];//获取主题
                                if (item.SubIds.Contains(client.LongId))
                                {
                                    var res = RedisTopic.RemoveSub(item, client);
                                }
                            }
                        }
                        await client.SendRedisOK();
                        return;
                    }
                    break;
                case RedisCommand.PUBLISH://发布 主题消息
                    {
                        var RedisQueueMessage = LinyeeWebSocketConnectionBase.RedisQueueMessage;
                        if (args.Length < 2)
                        {
                            await client.SendRedisFAIL("参数个数不正确");
                            return;
                        }

                        var RedisTopic = LinyeeWebSocketConnectionBase.RedisTopic;
                        var topic = args[0].body;
                        var msg = string.Join("\t", args.Skip(1).Select(p => p.body));

                        //主题
                        var tism = client.TopicRegex?.IsMatch(topic);
                        //Console.WriteLine("正则数据：{0} 检测结果：{1}", client.TopicRegex, tism);
                        var ism = tism == true;
                        if (!RedisTopic.ContainsKey(topic) && !ism)
                        {
                            await client.SendRedisFAIL(string.Format("不存在主题{0}，请先关注或创建", topic));
                            return;
                        }
                        else if (!RedisTopic.ContainsKey(topic) && ism)
                        {
                            var res = await client.AddTopic(topic);
                            if (!res.IsOk)
                            {
                                await client.SendRedisFAIL(string.Format("主题{0}，添加失败", topic));
                                return;
                            }
                        }

                        //队列消息
                        Queue<QueueMessage> item = null;
                        if (!RedisQueueMessage.ContainsKey(topic))
                        {
                            item = new Queue<QueueMessage>();
                            RedisQueueMessage.TryAdd(topic, item);
                        }
                        else
                        {
                            item = RedisQueueMessage[topic];
                        }

                        var quemsg = new QueueMessage()
                        {
                            p = topic,
                            u = client.LongId,
                            m = msg,
                            t = DateTime.Now.GetTimestamp(),
                            d = 0L,
                        };
                        item.Enqueue(quemsg);
                        var tpitem = RedisTopic[topic];

                        //发送队列消息的线程
                        new Thread(async () => {
                            var OnlineSockets = LinyeeWebSocketConnectionBase.OnlineSockets;
                            while (item.Count > 0)
                            {
                                var qmsg = item.Dequeue();
                                var quejson = qmsg.ToJsonString();//&& p.LongId != client.LongId//加上这个自己发的不接收
                                var qids = OnlineSockets.Values
                                .Where(p => (p.TypeCode / 100) % 100 == 1)//只向事件端发送数据
                                .Where(p => (quemsg.d == 0 && (tpitem.SubIds.Contains(p.LongId) || p.TopicRegex?.IsMatch(qmsg.p) == true)) || (quemsg.d > 0 && quemsg.d == p.LongId)).ToList();
                                foreach (var id in qids)
                                {
                                    try
                                    {
                                        await id.SendRedisArray(RedisCommand.PUBLISH.ToString(), qmsg.p, quejson);
                                    }
                                    catch (Exception ex)
                                    {
                                        var exmsg = LogService.Exception(ex);
                                        LogService.AnyLog("Topic", id.LongId.ToString(), topic, exmsg);
                                    }
                                }
                            }
                        }).Start();

                        await client.SendRedisOK();
                        return;
                    }
                    break;
                case RedisCommand.PUBSUB://获取主题信息
                    {
                        var RedisTopic = LinyeeWebSocketConnectionBase.RedisTopic;
                        if (args.Length < 1)
                        {
                            await client.SendRedisFAIL("参数个数不正确");
                            return;
                        }

                        var topic = args[0].body;
                        if (RedisTopic.ContainsKey(topic))
                        {
                            var item = RedisTopic[topic];//获取主题
                            if (item.LongId == client.LongId)
                            {
                                await client.SendRedisOK(item.ToJsonString());
                            }
                            else
                            {
                                await client.SendRedisOK(new { item.LongId, item.Topic, item.TypeCode, item.CreatedTime, item.LastMsgTime, }.ToJsonString());
                            }
                        }
                        else
                        {
                            await client.SendRedisFAIL(string.Format("不存在主题{0}，请先关注或创建", topic));
                        }
                    }
                    break;
                #endregion

                default:
                    await client.SendRedisUnkownCommand(cmd.body);//发送一个标志
                    break;
            }


        }
        #endregion

        #region "队列消息"
        /// <summary>
        /// 检测键名
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static async Task<ExecuteResult<string>> CheckRedisKeyResult(this LinyeeWebSocketConnectionBase client, string key)
        {
            ExecuteResult<string> result = new ExecuteResult<string>();
            //不能为空白
            if (string.IsNullOrWhiteSpace(key))
            {
                return result.SetFail().SetData(RedisErrorBuilder("键名不能空白"));
            }

            //键名过长
            if (key.Length >= 128)
            {
                return result.SetFail().SetData(RedisErrorBuilder("键名过长"));
            }

            //首字符必须字母 下划线
            var fc = key.ElementAt(0);
            if (!(fc >= 'a' && fc <= 'z' || fc >= 'A' && fc <= 'Z' || fc == '_'))
            {
                return result.SetFail().SetData(RedisErrorBuilder("键名首字符必须是字母"));
            }

            //不能有指定的字符
            if (REDISProtocol.keynochars.IsMatch(key))
            {
                await client.SendRedisFAIL("");
                return result.SetFail().SetData(RedisErrorBuilder("键名含有非法字符，只能使用\\w字符集"));
            }

            return result.SetOk();
        }
        /// <summary>
        /// 检测键名
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static async Task<bool> CheckRedisKey(this LinyeeWebSocketConnectionBase client, string key)
        {
            //不能为空白
            if (string.IsNullOrWhiteSpace(key))
            {
                await client.SendRedisFAIL("键名不能空白");
                return false;
            }

            //键名过长
            if (key.Length >= 128)
            {
                await client.SendRedisFAIL("键名过长");
                return false;
            }

            //首字符必须字母
            var fc = key.ElementAt(0);
            if (!(fc >= 'a' && fc <= 'z') || fc >= 'A' && fc <= 'Z')
            {
                await client.SendRedisFAIL("键名首字符必须是字母");
                return false;
            }

            //不能有指定的字符
            if (REDISProtocol.keynochars.IsMatch(key))
            {
                await client.SendRedisFAIL("键名含有非法字符，只能使用\\w字符集");
                return false;
            }

            return true;
        }
        #endregion

        #region "缓存"
        /// <summary>
        /// 检测键名
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        private static async Task<ExecuteResult<string>> AddTopic(this LinyeeWebSocketConnectionBase client, string topic)
        {
            ExecuteResult<string> result = new ExecuteResult<string>();
            var RedisTopic = LinyeeWebSocketConnectionBase.RedisTopic;
            var OnlineSockets = LinyeeWebSocketConnectionBase.OnlineSockets;

            if (!RedisTopic.ContainsKey(topic))
            {
                long TypeCode = client.TypeCode;
                if (TypeCode == 0) TypeCode = 1000000000000L;//1万亿
                else TypeCode *= 100000000L;//*1亿;
                var item = new RedisTopicInfo()
                {
                    Topic = topic,
                    LongId = client.LongId,
                    TypeCode = TypeCode,
                };
                item.SubIds.Add(client.LongId);//添加到关注名单
                RedisTopic.TryAdd(topic, item);//添加到主题字典
            }
            else
            {
                var item = RedisTopic[topic];//获取主题

                if (item.TypeCode >= client.TypeCode && item.TypeCode <= 1000000000000L)
                {
                    item.SubIds.Add(client.LongId);//添加到关注名单
                    if (item.TypeCode <= 1000000000000L && OnlineSockets.Count > 0 && !OnlineSockets.Values.Any(p => p.LongId == item.LongId)) item.LongId = OnlineSockets.Values.FirstOrDefault().LongId;//修改房主
                }
                else
                {
                    return result.SetFail(string.Format("{0} 此主题您无权关注", topic));
                }
            }

            return result.SetOk();
        }

        /// <summary>
        /// 是否存在键
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static async Task RedisExists(this LinyeeWebSocketConnectionBase client, string key)
        {
            if (LinyeeWebSocketConnectionBase.RedisCaching.ContainsKey(key))
            {
                await client.SendRedisInteger(1);
            }
            else
            {
                await client.SendRedisInteger(0);
            }
        }

        /// <summary>
        /// 动作队列
        /// </summary>
        /// <param name="client"></param>
        /// <param name="p"></param>
        private static async Task EnqueueRedisMulti(this LinyeeWebSocketConnectionBase client, Func<LinyeeWebSocketConnectionBase, RedisCachingInfo> mf)
        {
            var RedisMulti = LinyeeWebSocketConnectionBase.RedisMulti;
            RedisMultiInfo item = new RedisMultiInfo(client.LongId, client.MainTypeCode, mf);
            RedisMulti.TryAdd(item.Key, item);
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static RedisCachingInfo RedisSet(this LinyeeWebSocketConnectionBase client, string key, string value, long sec = 0)
        {
            var RedisCaching = LinyeeWebSocketConnectionBase.RedisCaching;
            var RedisCahingKeys = LinyeeWebSocketConnectionBase.RedisCahingKeys;

            var item = RedisCaching.Values.FirstOrDefault(p => p.Key == key);
            var maxLen = RedisCahingKeys.Values.Max(p => (long?)p.Length);//最大值
            var max2Len = maxLen / 2;//半值
            var cachekey = RedisCahingKeys.Values.FirstOrDefault(p => p.Key == key);
            var keyitem = LinyeeWebSocketConnectionBase.RedisDiskKeys.Values.FirstOrDefault(p => p.Key == key);
            var delkeyitem = LinyeeWebSocketConnectionBase.RedisDiskKeys.Values.FirstOrDefault(p => p.Key.EndsWith("~" + key));

            {

                //内存缓存
                LinyeeWebSocketConnectionBase.RedisOperCount++;
                LinyeeWebSocketConnectionBase.RedisOperTime = DateTime.Now;

                //更新缓存
                item = new RedisCachingInfo(key, value, sec);
                item = RedisCaching.AddOrUpdate(key,
                    (k) => {
                        if (item.ExpireSec > 0)
                        {
                            item.ExpireTime = DateTime.Now.AddSeconds(item.ExpireSec);
                        }
                        item.State = EntityState.Added;
                        return item;
                    },
                    (k, v) => {
                        v.SetValue(item.Value, item.HasFlater);
                        if (v.ExpireSec > 0)
                        {
                            v.ExpireTime = DateTime.Now.AddSeconds(v.ExpireSec);
                        }

                        if (item.State == EntityState.Unchanged) v.State = EntityState.Modified;
                        return v;
                    });

            }

            return item;
        }

        /// <summary>
        /// 获取队列值
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal static string RedisGetQue(this LinyeeWebSocketConnectionBase client, string key)
        {
            var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
            Queue<string> que = new Queue<string>();
            if (RedisQueue.ContainsKey(key)) que = RedisQueue[key];

            List<string> list = new List<string>();
            var ge = que.GetEnumerator();
            while (ge.MoveNext())
            {
                list.Add(ge.Current);
            }
            return string.Join(",", list);
        }

        /// <summary>
        /// 获取值
        /// 并移除过期的数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal static RedisCachingInfo RedisGet(this LinyeeWebSocketConnectionBase client, string key)
        {
            var RedisCaching = LinyeeWebSocketConnectionBase.RedisCaching;
            //Console.WriteLine("获取缓存");
            //标记过期项
            var dtnow = DateTime.Now;
            var expis = RedisCaching.Values.Where(p => p.ExpireTime <= dtnow).ToList();
            //RedisCachingInfo rout = null;
            foreach (var delitem in expis)
            {
                RedisCaching.AddOrUpdate(delitem.Key, delitem, (k, v) => {
                    v.State = EntityState.Deleted;
                    return v;
                });
            }

            var item = RedisCaching.Values.FirstOrDefault(p => p.Key == key);
            return item;
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static RedisCachingInfo RedisExpire(this LinyeeWebSocketConnectionBase client, string key, long sec)
        {
            RedisCachingInfo item = null;
            if (sec <= 0)
            {
                client.SendRedisFAIL("过期时间应于0");
                return item;
            }

            LinyeeWebSocketConnectionBase.RedisCaching.TryGetValue(key, out item);
            if (item == null)
            {
                client.SendRedisFAIL("指定项不存在");
                return item;
            }

            item.ExpireSec = sec;
            if (item.ExpireSec > 0)
            {
                item.ExpireTime = DateTime.Now.AddSeconds(item.ExpireSec);
            }
            return item;
        }
        #endregion

        #endregion
    }
}

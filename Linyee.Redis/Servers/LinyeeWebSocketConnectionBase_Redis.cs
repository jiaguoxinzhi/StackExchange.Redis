using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WS_Core.BLL;
using WS_Core.Bodys;
using WS_Core.Bodys.Json;
using WS_Core.Bodys.Redis;
using WS_Core.Consts;
using WS_Core.dyCompiler;
using WS_Core.Enums;
using WS_Core.Tools;

namespace WS_Server.Servers
{
    /// <summary>
    /// 客户端Redis消息
    /// </summary>
    /// <remarks>
    /// <!--注意事项-->
    /// <!--所有的请求都要发送一次数据给客户端，而且很多情况下只能发送一次。只有主客户端跟事件端是支持双向沟通的。-->
    /// </remarks>
    [Author("Linyee", "2019-02-01")]
    public static class LinyeeWebSocketConnectionBase_Redis
    {
        #region "发送消息"
        /// <summary>
        /// 发送一个Redis成功数据
        /// </summary>
        public static async Task SendRedisOK(this LinyeeWebSocketConnectionBase client,string msg="OK")
        {
            await client.SendMsg(string.Format("+{0}\r\n",msg));
        }
        /// <summary>
        /// 发送一个Redis失败数据
        /// </summary>
        public static async Task SendRedisFAIL(this LinyeeWebSocketConnectionBase client,string msg="")
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
        public static async Task SendRedisInteger(this LinyeeWebSocketConnectionBase client,long count)
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
        public static async Task SendRedisOKBulk(this LinyeeWebSocketConnectionBase client,string msg, string okmsg = "OK")
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
        public static async Task SendRedisArray(this LinyeeWebSocketConnectionBase client,params string[] args )
        {
            if (args != null || args.LongLength>0)
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
                sbd.Append(args.LongLength+1);
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
        public static string  RedisArrayBuilder(params string[] args)
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
        public static async Task SendRedisNonSupportAction(this LinyeeWebSocketConnectionBase client,char c)
        {
            await client.SendMsg(string.Format("-Non Support Action '{0}'\r\n", c));
        }

        /// <summary>
        /// 发送一个Redis未知命令数据
        /// </summary>
        public static async Task SendRedisUnkownCommand(this LinyeeWebSocketConnectionBase client,string cmd)
        {
            await client.SendMsg(string.Format("-Non Unkown Command '{0}'\r\n", cmd));
        }
        #endregion

        #region "redis协议"

        #region "Multi 事务"

        /// <summary>
        /// 清空事务
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async void RedisMultiClear(this LinyeeWebSocketConnectionBase client)
        {
            var RedisMulti = LinyeeWebSocketConnectionBase.RedisMulti;
            foreach (var item in RedisMulti.Values.Where(p => p.LongId == client.LongId && p.MainTypeCode == client.MainTypeCode).OrderBy(p => p.Ticks).ToArray())
            {
                RedisMultiInfo delitem = null;
                RedisMulti.TryRemove(item.Key, out delitem);
            }
            await client.SendRedisOK();
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="client"></param>
        /// <param name="sponse">是否需要响应，默认需要</param>
        /// <returns></returns>
        public static async Task RedisMultiEXEC(this LinyeeWebSocketConnectionBase client,bool sponse=true)
        {
            var RedisMulti = LinyeeWebSocketConnectionBase.RedisMulti;
            var RedisMultis = RedisMulti.Values.Where(p => p.LongId == client.LongId && p.MainTypeCode == client.MainTypeCode).OrderBy(p => p.Ticks).ToArray();
            foreach (var item in RedisMultis)
            {
                RedisMultiInfo delitem = null;
                RedisMulti.TryRemove(item.Key, out delitem);

                item.MultiFunc?.Invoke(client);//执行委托
            }
            if (RedisMultis.Length > 1)
            {
                var dt1 = RedisMultis[0].Ticks;
                var dt2= RedisMultis[RedisMultis.Length-1].Ticks;
                var timesp = new TimeSpan(dt2 - dt1);
                if(sponse) await client.SendRedisOK("事务执行时间"+ timesp.TotalMilliseconds+"毫秒");
            }
            else
            {
                if (sponse) await client.SendRedisOK();
            }
        }
        /// <summary>
        /// 事务队列的总量
        /// </summary>
        /// <param name="client"></param>
        public static int RedisMultiCount(this LinyeeWebSocketConnectionBase client)
        {
            var RedisMulti = LinyeeWebSocketConnectionBase.RedisMulti;
            return RedisMulti.Values.Where(p => p.LongId == client.LongId && p.MainTypeCode == client.MainTypeCode).Count();
        }
        #endregion

        #region "Redis 命令"
        /// <summary>
        /// 运行Redis
        /// </summary>
        /// <param name="client"></param>
        /// <param name="rdmsg"></param>
        /// <returns></returns>
        public static async Task RunCommand(this LinyeeWebSocketConnectionBase client, List<REDISClientCommand> rdmsg)
        {
            List<string> list = new List<string>();
            foreach(var cmd in rdmsg)
            {
                var res =RunCommandResult(client,cmd).Result;
                list.Add(res.Data);
            }
            await client.SendMsg(string.Join("",list));
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
                #endregion

                #endregion

                #region "缓存"
                //case RedisCommand.MULTI:
                //    {
                //        if (client.IsRedisMulti)
                //        {
                //            return result.SetData(RedisErrorBuilder("先前的事务未完成"));
                //        }
                //        else
                //        {
                //            client.IsRedisMulti = true;
                //            return result.SetData(REDISProtocol.OKString);
                //        }
                //    }
                //    break;
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
                case RedisCommand.GET:
                    {
                        var res = client.RedisGet(args[0].body);
                        if (res != null) return result.SetData(RedisBulkBuilder(res.GetValue()));
                        else return result.SetData(RedisBulkBuilder(null));
                    }
                //case RedisCommand.EXPIRE:
                //    {
                //        if (client.IsRedisMulti)
                //        {
                //            await client.EnqueueRedisMulti((c) => c.RedisExpire(args[0].body, args[1].body));
                //        }
                //        else
                //        {
                //            var res = client.RedisExpire(args[0].body, args[1].body);
                //            if (res != null) return result.SetData(REDISProtocol.OKString);
                //        }
                //    }
                //    break;
                //case RedisCommand.EXEC:
                //    {
                //        await client.RedisMultiEXEC();
                //    }
                //    break;
                //case RedisCommand.DISCARD:
                //    {
                //        client.IsRedisMulti = false;//清空事务
                //    }
                //    break;
                //case RedisCommand.EXISTS:
                //    {
                //        await client.RedisExists(args[0].body);
                //    }
                //    break;
                case RedisCommand.EVAL:
                    {
                        var input = "";
                        if (args.Length > 0) input = args[0].body;
                        var rr = new RedisRepalce(client, input);
                        var text = rr.valText;
                        var res = rr.result;
                        if (res.IsOk)
                        {
                            var val = text.eval();
                            //await client.SendRedisOK(val.ToJsonString());
                            if (val.Type == EvalObjectType.Error)
                            {
                                return result.SetData(RedisErrorBuilder(val.Value?.ToString()));
                            }
                            else
                            {
                                return result.SetData(string.Format(":{0}\r\n", val.Value));
                            }
                        }
                        else
                        {
                            return result.SetData(RedisErrorBuilder(res.Msg));
                        }
                    }

                #endregion

                #region "服务端"
                case RedisCommand.AUTH:
                    {
                        if (args.Length < 4)
                        {
                            return result.SetData(RedisErrorBuilder("参数不足"));
                        }

                        var mqquest = new MQProtocol<MQReQuestBase<LoginRequest>>()
                        {
                            Action = MQProtocolActionType.Redis,
                            Command = MQProtocolCommandType.AUTH,
                        };
                        var body = mqquest.Data.Body;
                        body.loginAccount = args[0].body;
                        body.loginPassword = args[1].body;
                        body.timestamp = long.Parse(args[2].body);
                        body.sign = args[3].body;
                        if (args.Length >= 5) body.loginType = int.Parse(args[4].body);

                        await client.RedisLogin(body, mqquest.Data.RequestId);
                        return result.SetData(REDISProtocol.OKString);
                    }
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
                    return result.SetData(RedisErrorBuilder("non super "+ cmd.body));
            }
            return result.SetData(RedisErrorBuilder("error excuting " + cmd.body));
        }

        /// <summary>
        /// 运行Redis
        /// </summary>
        /// <param name="client"></param>
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
        /// <param name="client"></param>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task RunCommand(this LinyeeWebSocketConnectionBase client, REDISBulk cmd,params REDISBulk[] args)
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
                                client.Name= args[1].body;
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
                                    await client.SendRedisArray(key,val.Data?.ToString());
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
                        if (! await client.CheckRedisKey(args[0].body))
                        {
                            return;
                        }

                        var RedisMulti = LinyeeWebSocketConnectionBase.RedisMulti;
                        string msg = "OK";
                        //如果内存已有80% 或已有1000次，则先执行现有事务
                        if (CpuCount_Helper.GetCurMemUseRate() > REDISProtocol.RedisMaxMemUseRate || client.RedisMultiCount()>= REDISProtocol.RedisOperMaxCount )
                        {
                            await client.RedisMultiEXEC(false);
                            client.IsRedisMulti = true;
                            msg += " 事务已达标，自动执行完成";
                        }

                        if(args.Length<2)
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
                case RedisCommand.EXPIRE:
                    {
                        if (client.IsRedisMulti)
                        {
                            await client.EnqueueRedisMulti((c) => c.RedisExpire(args[0].body, args[1].body).Result);
                        }
                        else
                        {
                            var res = client.RedisExpire(args[0].body, args[1].body);
                            if (res != null) await client.SendRedisOK();
                        }
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
                case RedisCommand.EVAL:
                    {
                        var input = "";
                        if (args.Length > 0) input = args[0].body;
                         var rr=new RedisRepalce(client, input);
                        var text = rr.valText;
                        var res = rr.result;
                        if (res.IsOk)
                        {
                            var val = text.eval();
                            //await client.SendRedisOK(val.ToJsonString());
                            if(val.Type== EvalObjectType.Error)
                            {
                                await client.SendRedisFAIL(val.Value?.ToString());
                            }
                            else
                            {
                                await client.SendRedisInteger(val.Value);
                            }
                        }
                        else
                        {
                            await client.SendRedisFAIL(res.Msg);
                        }
                    }
                    break;

                #endregion

                #region "服务端"
                case RedisCommand.AUTH:
                    {
                        if (args.Length < 4)
                        {
                            await client.SendRedisFAIL("登录参数不足");
                            return;
                        }

                        var mqquest = new MQProtocol<MQReQuestBase<LoginRequest>>()
                        {
                            Action = MQProtocolActionType.Redis,
                            Command = MQProtocolCommandType.AUTH,
                        };
                        var body = mqquest.Data.Body;
                        body.loginAccount = args[0].body;
                        body.loginPassword = args[1].body;
                        body.timestamp = long.Parse(args[2].body);
                        body.sign = args[3].body;
                        if (args.Length >= 5) body.loginType = int.Parse(args[4].body);

                        await client.RedisLogin(body, mqquest.Data.RequestId);
                    }
                    break;
                case RedisCommand.PING:
                    {
                        await client.SendRedisOK("PONG");
                    }
                    break;
                case RedisCommand.SAVE:
                    {
                        var res= await client.RedisSave();
                        if (res.IsOk)
                        {
                            await client.SendRedisOK(res.Msg+ "，数据库总字节数：" + res.Data.ToGMKB());
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
                        var val= args[1].body;
                        Queue<string> que = new Queue<string>();
                        if (!RedisQueue.ContainsKey(key)) RedisQueue.TryAdd(key, que);
                        else que = RedisQueue[key];
                        que.Enqueue(val);
                        await client.SendRedisOK();
                        return;
                    }
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

                            if (topic.IndexOf("*")>=0 && topic.IndexOf("#") >= 0)
                            {
                                lsb.Add(string.Format("{0} 主题不能#*同时存在", topic));
                                continue;
                            }

                            //泛关注 只保留最后一次
                            if (topic.IndexOf("#") >= 0)
                            {
                                client.Topic="^"+ topic.Replace("#", "[\\w\\/]*")+"$";
                                //更新到其它端
                                OnlineSockets.Values.Where(p => p.LongId == client.LongId && p.ClientId != client.ClientId)
                                    .Select(p=> {
                                        p.Topic = client.Topic;
                                    return p;
                                }).ToList();
                                continue;
                            }

                            //现有正则关注
                            var regtopic = topic.Replace("*", "[\\w]+");
                            Regex topicrgx = new Regex(regtopic, RegexOptions.Compiled);

                            var tpcs = RedisTopic.Keys.Where(p=> topicrgx.IsMatch(p));
                            foreach(var tpc in tpcs)
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

                            var res= await client.AddTopic(topic);
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
                        var msg =string.Join("\t", args.Skip(1).Select(p => p.body));

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
                            var res= await client.AddTopic(topic);
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
                            p=topic,
                            u= client.LongId,
                            m = msg,
                            t=DateTime.Now.GetTimestamp(),
                            d=0L,
                        };
                        item.Enqueue(quemsg);
                        var tpitem = RedisTopic[topic];

                        //发送队列消息的线程
                        new Thread(async() => {
                            var OnlineSockets = LinyeeWebSocketConnectionBase.OnlineSockets;
                            while (item.Count > 0)
                            {
                                var qmsg = item.Dequeue();
                                var quejson = qmsg.ToJsonString();//&& p.LongId != client.LongId//加上这个自己发的不接收
                                var qids = OnlineSockets.Values
                                .Where(p=>(p.TypeCode /100)%100==1)//只向事件端发送数据
                                .Where(p => (quemsg.d==0 && (tpitem.SubIds.Contains(p.LongId)|| p.TopicRegex?.IsMatch(qmsg.p)==true) ) || (quemsg.d>0 && quemsg.d==p.LongId)).ToList();
                                foreach(var id in qids)
                                {
                                    try
                                    {
                                        await id.SendRedisArray(RedisCommand.PUBLISH.ToString(),qmsg.p, quejson);
                                    }
                                    catch (Exception ex)
                                    {
                                        var exmsg= LogService.Exception(ex);
                                        LogService.AnyLog("Topic", id.LongId.ToString(), topic, exmsg);
                                    }
                                }
                            }
                        }).Start();

                        await client.SendRedisOK();
                        return;
                    }
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
                            if(item.LongId== client.LongId)
                            {
                                await client.SendRedisOK(item.ToJsonString());
                            }
                            else
                            {
                                await client.SendRedisOK(new { item.LongId,item.Topic,item.TypeCode,item.CreatedTime,item.LastMsgTime,}.ToJsonString());
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
        /// <param name="client"></param>
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
            if (!(fc >= 'a' && fc <= 'z' || fc >= 'A' && fc <= 'Z' || fc=='_'))
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
        /// <param name="client"></param>
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

        #region "保存"
        /// <summary>
        /// 检查指标，达标时自动保存
        /// </summary>
        private static void CheckSave()
        {
            if (LinyeeWebSocketConnectionBase.RedisOperCount >= REDISProtocol.RedisOperMaxCount)
            {
                new Thread(async() =>
                {
                    await RedisSave();
                }).Start();
            }
        }

        /// <summary>
        /// 是否正在保存
        /// </summary>
        private static bool SaveRuing=false;
        /// <summary>
        /// 数据所在目录
        /// </summary>
        private static string DbPath = LinyeeWebSocketConnectionBase.DbPath;

        /// <summary>
        /// 持久化 内存表 硬盘键表
        /// </summary>
        /// <param name="client"></param>
        public static async Task<ExecuteResult<long>> RedisSave(this LinyeeWebSocketConnectionBase client)
        {
            return await RedisSave();
        }

        /// <summary>
        /// 持久化 内存表 硬盘键表 主题表
        /// </summary>
        public static async Task<ExecuteResult<long>> RedisSave()
        {
            ExecuteResult<long> result = new ExecuteResult<long>();
            if (SaveRuing)
            {
                var msg= LogService.Runtime("已有保存任务正在运行。请稍候。。");
                return result.SetFail(msg);
            }
            SaveRuing = true;//锁定防止 重复保存
            LinyeeWebSocketConnectionBase.RedisOperCount = 0;//重置操作次数

            result=await RedisSaveDb();
            await RedisSaveDbDiskKeys();
            await RedisSaveDbTopic();

            Console.WriteLine(DateTime.Now.ToString("dd HH:mm:ss") + "\t数据保存完成");
            LogService.Runtime("数据保存完成");

            LinyeeWebSocketConnectionBase.RedisSaveTime = DateTime.Now;//更新最后保存时间
            SaveRuing = false;

            return result;
        }

        #endregion

        #region "保存主题"
        /// <summary>
        /// 保存主题
        /// </summary>
        /// <returns></returns>
        private static async Task RedisSaveDbTopic()
        {
            var filename = "dbtopics.data";
            var dictname = "主题";
            var RedisTopic = LinyeeWebSocketConnectionBase.RedisTopic;
            await RedisSaveDbTopic(RedisTopic, filename, dictname);
        }

        /// <summary>
        /// 保存主题
        /// </summary>
        private static async Task RedisSaveDbTopic(ConcurrentDictionary<string, RedisTopicInfo> dict, string filename, string dictname)
        {
            if (dict.Count < 0) return;

            var DbValueFile = Path.Combine(DbPath, filename);
            if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);

            //覆盖写入
            using (var valfile = new FileStream(DbValueFile, FileMode.Create, FileAccess.Write, FileShare.Write, 1024 * 1024 * 16))
            {
                var json =await dict.Values.ToJsonStringAsync();
                using (var sw = new StreamWriter(valfile, Encoding.UTF8))
                {
                    await sw.WriteAsync(json);
                }
            }

            Console.WriteLine("{0}\t{1}保存完成",DateTime.Now.TimeOfDay, dictname);
        }
        #endregion

        #region "保存索引"
        /// <summary>
        /// 保存硬盘键表
        /// 硬盘数据索引
        /// </summary>
        private static async Task RedisSaveDbDiskKeys()
        {
            var RedisDiskKeys = LinyeeWebSocketConnectionBase.RedisDiskKeys;
            var filenae = "dbdiskkeys.data";
            await RedisSaveKeys(RedisDiskKeys, filenae);
        }

        /// <summary>
        /// 保存内存键表
        /// 内存数据索引
        /// </summary>
        /// <returns></returns>
        private static async Task RedisSaveDbKeys()
        {
            var RedisCahingKeys = LinyeeWebSocketConnectionBase.RedisCahingKeys;
            var filenae = "dbkeys.data";
            await RedisSaveKeys(RedisCahingKeys, filenae);
        }


        /// <summary>
        /// 空转
        /// </summary>
        private static Task VoidTask()
        {
            return Task.Run(() =>
            {
            });
        }

        /// <summary>
        /// 保存数据索引
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="filename"></param>
        private static async Task RedisSaveKeys(ConcurrentDictionary<string, RedisCachingKeyInfo> dict,string filename)
        {
            //空转
            await VoidTask();

            if (dict.Count < 0) return ;
            //组装数据
            StringBuilder valsbd = new StringBuilder();
            var dtnow = DateTime.Now;

            //var DbValueFile = Path.Combine(DbPath, "dbkeys.data");
            var DbValueFile = Path.Combine(DbPath, filename);
            if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);

            //Console.WriteLine("内存索引数据："+dict.ToJsonString());

            //覆盖写入
            using (var valfile = new FileStream(DbValueFile, FileMode.Create, FileAccess.Write, FileShare.Write, 1024 * 1024 * 16))
            {
                using (var bw = new BinaryWriter(valfile, Encoding.UTF8))
                {
                    foreach (var fc in REDISProtocol.FirstCharsDel)
                    {
                        foreach (var keyItem in dict.Values.Where(p => p.Key.StartsWith(""+fc)).OrderBy(p => p.Key).ToArray())
                        {
                            bw.WriteLine(keyItem.Key)
                            .WriteLong(keyItem.Offset)
                            .WriteLong(keyItem.Length)
                            .WriteByte(keyItem.Status);
                        }
                    }
                }
            }
        }

        #endregion

        #region "硬盘写入"

        /// <summary>
        /// 设置值
        /// 硬盘数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        private static RedisCachingInfo RedisSetDisk(this LinyeeWebSocketConnectionBase client, string key, string value, long sec)
        {
            var RedisDiskKeys = LinyeeWebSocketConnectionBase.RedisDiskKeys;

            //压缩
            var item = new RedisCachingInfo(key, value, sec);
            var buf = item.ToJsonString().GetUTF8Bytes();
            var Flaterbuf = buf.FlaterCompress();
            int len = Flaterbuf.Length;

            if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);
            var DbKeyFile = Path.Combine(DbPath, "dbdiskkeys.data");
            var DbValueFile = Path.Combine(DbPath, "dbdiskvalues.data");

            var keyitem = RedisDiskKeys.Values.FirstOrDefault(p => p.Key == key);
            if (keyitem != null)
            {
                client.RedisSetDisk(keyitem, value, sec);
            }
            else
            {
                //不存时创建
                if (!File.Exists(DbKeyFile))
                {
                    using (var ms = new MemoryStream())
                    {
                        File.WriteAllText(DbKeyFile, "");
                    }
                }

                //不存时创建
                if (!File.Exists(DbValueFile))
                {
                    using (var ms = new MemoryStream())
                    {
                        File.WriteAllText(DbValueFile, "");
                    }
                }

                //追加写入
                //键
                using (var keyfile = new FileStream(DbKeyFile, FileMode.Append, FileAccess.Write, FileShare.Write, 1024 * 1024 * 16))
                {
                    //值
                    using (var valfile = new FileStream(DbValueFile, FileMode.Append, FileAccess.Write, FileShare.Write, 1024 * 1024 * 16))
                    {
                        var offset = valfile.Length;
                        var keyset = new RedisCachingKeyInfo(key, offset, len);

                        using (var bwkey = new BinaryWriter(keyfile))
                        {
                            bwkey.WriteLine(key)
                            .WriteLong(offset)
                            .WriteLong(len);
                        }

                        using (var bwval = new BinaryWriter(valfile))
                        {
                            bwval.Write(len);
                            bwval.Write(Flaterbuf);
                        }

                        //添加键值
                        RedisDiskKeys.TryAdd(key, keyset);
                    }
                }
            }

            return item;
        }

        /// <summary>
        /// 更新值
        /// 硬盘数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="keyitem"></param>
        /// <param name="value"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        private static RedisCachingInfo RedisSetDisk(this LinyeeWebSocketConnectionBase client, RedisCachingKeyInfo keyitem, string value, long sec)
        {
            if (keyitem == null) throw new Exception("更新硬盘数据时，必须指定已有索引");
            var RedisDiskKeys = LinyeeWebSocketConnectionBase.RedisDiskKeys;
            var key = keyitem.Key;

            //压缩
            var item = new RedisCachingInfo(keyitem.Key, value, sec);
            var buf = item.ToJsonString().GetUTF8Bytes();
            var Flaterbuf = buf.FlaterCompress();
            int len = Flaterbuf.Length;

            if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);
            var DbKeyFile = Path.Combine(DbPath, "dbdiskkeys.data");
            var DbValueFile = Path.Combine(DbPath, "dbdiskvalues.data");

            //覆盖写入
            if (keyitem.Length >= len)
            {
                //值
                using (var valfile = new FileStream(DbValueFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 1024 * 1024 * 16))
                {
                    valfile.Position = keyitem.Offset;
                    using (var bwval = new BinaryWriter(valfile))
                    {
                        bwval.Write(len);
                        bwval.Write(Flaterbuf);
                    }
                }
            }
            //重新写入
            else
            {
                LinyeeWebSocketConnectionBase.RedisOperCount++;
                LinyeeWebSocketConnectionBase.RedisOperTime = DateTime.Now;

                using (var valfile = new FileStream(DbValueFile, FileMode.Append, FileAccess.Write, FileShare.Write, 1024 * 1024 * 16))
                {
                    keyitem.Key = "~" + DateTime.Now.Ticks + "~" + keyitem.Key;//删除原鍵
                    keyitem.Status = 0;
                    RedisCachingKeyInfo delkey = null;
                    RedisDiskKeys.TryRemove(key, out delkey);
                    RedisDiskKeys.TryAdd(keyitem.Key, keyitem);
                    var offset = valfile.Length;
                    var keyset = new RedisCachingKeyInfo(key, offset, len);

                    using (var bwval = new BinaryWriter(valfile))
                    {
                        bwval.Write(len);
                        bwval.Write(Flaterbuf);
                    }

                    //添加键值
                    RedisDiskKeys.TryAdd(key, keyset);
                }

                CheckSave();//检查保存
            }

            return item;
        }

        /// <summary>
        /// 从已删除的恢复
        /// 硬盘数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="keyitem"></param>
        /// <param name="value"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
        private static RedisCachingInfo RedisSetDiskRestore(this LinyeeWebSocketConnectionBase client, RedisCachingKeyInfo keyitem, string value, long sec)
        {
            if (keyitem == null) throw new Exception("更新硬盘数据时，必须指定已有索引");
            var RedisDiskKeys = LinyeeWebSocketConnectionBase.RedisDiskKeys;
            var key = keyitem.Key;
            var newkey = keyitem.Key.Substring(keyitem.Key.LastIndexOf("~") + 1);//恢复原鍵

            //压缩
            var item = new RedisCachingInfo(keyitem.Key, value, sec);
            var buf = item.ToJsonString().GetUTF8Bytes();
            var Flaterbuf = buf.FlaterCompress();
            int len = Flaterbuf.Length;

            if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);
            var DbKeyFile = Path.Combine(DbPath, "dbdiskkeys.data");
            var DbValueFile = Path.Combine(DbPath, "dbdiskvalues.data");

            LinyeeWebSocketConnectionBase.RedisOperCount++;
            LinyeeWebSocketConnectionBase.RedisOperTime = DateTime.Now;

            //覆盖写入
            if (keyitem.Length >= len)
            {
                keyitem.Key = newkey;
                keyitem.Status = 1;
                RedisCachingKeyInfo delkey = null;
                RedisDiskKeys.TryRemove(key, out delkey);
                RedisDiskKeys.TryAdd(newkey, keyitem);

                //值
                using (var valfile = new FileStream(DbValueFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 1024 * 1024 * 16))
                {
                    valfile.Position = keyitem.Offset;
                    using (var bwval = new BinaryWriter(valfile))
                    {
                        bwval.Write(len);
                        bwval.Write(Flaterbuf);
                    }
                }
            }
            //重新写入
            else
            {
                using (var valfile = new FileStream(DbValueFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 1024 * 1024 * 16))
                {
                    var offset = valfile.Length;
                    var keyset = new RedisCachingKeyInfo(newkey, offset, len);

                    using (var bwval = new BinaryWriter(valfile))
                    {
                        bwval.Write(len);
                        bwval.Write(Flaterbuf);
                    }

                    //添加键值
                    RedisDiskKeys.TryAdd(newkey, keyset);
                }
            }

            CheckSave();//检查保存
            return item;
        }
        #endregion

        #region "内存数据写入硬盘"
        ///// <summary>
        ///// 保存内存数据库
        ///// </summary>
        //private static async Task<ExecuteResult<long>> RedisSaveDb()
        //{
        //    ExecuteResult<long> result = new ExecuteResult<long>();
        //    var DbValueFile = Path.Combine(DbPath, "dbvalue.data");
        //    if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);

        //    //部分覆盖写入+追加写入
        //    using (var valfile = new FileStream(DbValueFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 1024 * 1024 * 16))
        //    {
        //        //组装数据
        //        StringBuilder valsbd = new StringBuilder();
        //        var dtnow = DateTime.Now;
        //        foreach (var key in LinyeeWebSocketConnectionBase.RedisCaching.Keys.ToArray())
        //        {
        //            RedisCachingInfo tmpval = null;
        //            LinyeeWebSocketConnectionBase.RedisCaching.TryGetValue(key, out tmpval);
        //            if (tmpval == null)
        //            {
        //                valsbd.AppendLine();
        //            }
        //            else
        //            {
        //                if (tmpval.ExpireTime < dtnow) continue;//跳过已失效的记录
        //                valsbd.AppendLine(tmpval.ToJsonString());
        //            }

        //            //16M写入一次
        //            if (valsbd.Length >= 1024 * 1024 * 16)
        //            {
        //                valfile.Write(Encoding.UTF8.GetBytes(valsbd.ToString()));
        //                valsbd.Clear();
        //            }
        //        }

        //        //余尾写入一次
        //        if (valsbd.Length > 0)
        //        {
        //            valfile.Write(Encoding.UTF8.GetBytes(valsbd.ToString()));
        //        }

        //        result.SetOk("内存数据持久化完成", valfile.Length);
        //    }

        //    await RedisSaveDbKeys();//保存索引表
        //    return result;
        //}


        /// <summary>
        /// 保存内存数据库
        /// </summary>
        private static async Task<ExecuteResult<long>> RedisSaveDb()
        {
            var RedisCahingKeys = LinyeeWebSocketConnectionBase.RedisCahingKeys;
            var RedisCaching= LinyeeWebSocketConnectionBase.RedisCaching;

            ExecuteResult<long> result = new ExecuteResult<long>();
            var DbValueFile = Path.Combine(DbPath, "dbvalue.data");
            if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);
            var dtnow = DateTime.Now;

            //部分覆盖写入+追加写入
            using (var valfile = new FileStream(DbValueFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 1024 * 1024 * 16))
            {
                //回收覆盖写入  覆盖写入 追加写入
                bool hasnew = false;
                using (var bw = new BinaryWriter(valfile))
                {
                    //回收覆盖写入 向回收的键写入
                    foreach (var cacheItem in RedisCaching.Values.Where(p => p.State == EntityState.Added && p.ExpireTime > dtnow).OrderBy(p => p.Key).ToArray())
                    {
                        //压缩
                        var key = cacheItem.Key;
                        var json = cacheItem.ToJsonString();
                        var buf = Encoding.UTF8.GetBytes(json);
                        var Flaterbuf = buf.FlaterCompress();
                        int len = Flaterbuf.Length;


                        //回收索引
                        var delItem = RedisCahingKeys.Values.FirstOrDefault(p =>( p.Key.StartsWith("~") || p.Status==0) && p.Length >= len && p.Length <= (len + len / 10));
                        if (delItem == null) continue;

                        //更新索引
                        RedisCahingKeys.TryRemove(delItem.Key, out delItem);
                        delItem.Key = key;
                        delItem.Status = 1;
                        RedisCahingKeys.TryAdd(key, delItem);

                        //写入硬盘
                        bw.Write(len);
                        bw.Write(Flaterbuf);

                        //更新缓存
                        RedisCaching.AddOrUpdate(key, cacheItem, (k, v) => {
                            v.State = EntityState.Unchanged;
                            return v;
                        });
                    }
                    //更新到文件
                    //valfile.Flush();

                    //覆盖写入 修改过的 新增加的 未过期 按键名排序
                    foreach (var cacheItem in RedisCaching.Values.Where(p => p.State == EntityState.Modified && p.ExpireTime > dtnow).OrderBy(p => p.Key).ToArray())
                    {
                        //压缩
                        var key = cacheItem.Key;
                        var json = cacheItem.ToJsonString();
                        var buf = Encoding.UTF8.GetBytes(json);
                        var Flaterbuf = buf.FlaterCompress();
                        int len = Flaterbuf.Length;

                        var keyItem = RedisCahingKeys.Values.FirstOrDefault(p => p.Key == key);
                        //修改为添加
                        if (keyItem == null)
                        {
                            RedisCaching.AddOrUpdate(key, cacheItem, (k, v) =>
                             {
                                 hasnew = true;
                                 v.State = EntityState.Added;
                                 return v;
                             });
                        }
                        //更新数据
                        if (keyItem.Length >= len)
                        {
                            //更新缓存状态
                            RedisCaching.AddOrUpdate(key, cacheItem, (k, v) => {
                                v.State = EntityState.Unchanged;
                                return v;
                            });

                            //设置偏移
                            valfile.Position = keyItem.Offset;
                            //写入硬盘
                            bw.Write(len);
                            bw.Write(Flaterbuf);
                        }
                        //转为添加的数据，下次更新
                        else
                        {
                            //原键改为删除键
                            RedisCahingKeys.TryRemove(key, out keyItem);
                            var delkey = "~" + DateTime.Now.Ticks + "~" + key;
                            keyItem.Key = delkey;
                            keyItem.Status = 0;
                            RedisCahingKeys.TryAdd(delkey, keyItem);

                            //转为下次更新
                            RedisCaching.AddOrUpdate(key, cacheItem, (k, v) =>
                            {
                                hasnew = true;
                                v.State = EntityState.Added;
                                return v;
                            });

                            LinyeeWebSocketConnectionBase.RedisOperCount++;//增加操作次数
                        }
                    }
                    //更新到文件
                    valfile.Flush();

                    //追加写入
                    using (var ms = new MemoryStream())
                    {
                        using (var msbw = new BinaryWriter(ms))
                        {
                            //追加 新增加的 未过期 按键名排序
                            foreach (var cacheItem in RedisCaching.Values.Where(p => p.State == EntityState.Added && p.ExpireTime > dtnow).OrderBy(p => p.Key).ToArray())
                            {
                                //压缩
                                var key = cacheItem.Key;
                                var json = cacheItem.ToJsonString();
                                var buf = Encoding.UTF8.GetBytes(json);
                                var Flaterbuf = buf.FlaterCompress();
                                int len = Flaterbuf.Length;

                                //更新索引
                                var keyItem = new RedisCachingKeyInfo(key, valfile.Length + ms.Length, len);
                                RedisCahingKeys.TryAdd(key, keyItem);

                                //写入内存
                                msbw.Write(len);
                                msbw.Write(Flaterbuf);
                                msbw.Flush();

                                //更新缓存状态
                                RedisCaching.AddOrUpdate(key, cacheItem, (k, v) => {
                                    v.State = EntityState.Unchanged;
                                    return v;
                                });
                            }

                            //更新到文件
                            valfile.Position = valfile.Length;//这里要先设置到尾部
                            valfile.Write(ms.GetBuffer(), 0, (int)ms.Length);
                            valfile.Flush();
                        }

                    }

                    result.SetOk("内存数据持久化完成", valfile.Length);
                }

                //删除的 已过期 按键名排序
                foreach (var cacheItem in RedisCaching.Values.Where(p => p.State == EntityState.Deleted || p.ExpireTime < dtnow).OrderBy(p => p.Key).ToArray())
                {
                    var key = cacheItem.Key;
                    var keyItem = RedisCahingKeys.Values.FirstOrDefault(p => p.Key == key);

                    //原键改为删除键
                    RedisCahingKeys.TryRemove(key, out keyItem);
                    var delkey = "~" + DateTime.Now.Ticks + "~" + key;
                    keyItem.Key = delkey;
                    keyItem.Status = 0;
                    RedisCahingKeys.TryAdd(delkey, keyItem);

                    //删除缓存
                    RedisCachingInfo delitem = null;
                    RedisCaching.TryRemove(key, out delitem);
                }

                if (hasnew) return await RedisSaveDb();//重启保存新增
            }

            await RedisSaveDbKeys();//保存索引表
            return result;
        }
        #endregion

        #region "缓存"
        /// <summary>
        /// 检测键名
        /// </summary>
        /// <param name="client"></param>
        /// <param name="topic"></param>
        /// <returns></returns>
        private static async Task<ExecuteResult<string>> AddTopic(this LinyeeWebSocketConnectionBase client, string topic)
        {
            //空转
            await VoidTask();

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
        /// 执行事务
        /// </summary>
        /// <param name="client"></param>
        /// <param name="RedisMulti"></param>
        /// <returns></returns>
        public static async Task RunCommand(this LinyeeWebSocketConnectionBase client, ConcurrentQueue<Func<LinyeeWebSocketConnectionBase, RedisCachingInfo>> RedisMulti)
        {
            while (!RedisMulti.IsEmpty)
            {
                Func<LinyeeWebSocketConnectionBase, RedisCachingInfo> mf = null;
                RedisMulti.TryDequeue(out mf);
                if (mf != null)
                {
                    var res = mf(client);
                    if (res == null)
                    {
                        return;
                    }
                }
                await client.SendRedisOK();
            }
        }

        /// <summary>
        /// 动作队列
        /// </summary>
        /// <param name="client"></param>
        /// <param name="mf"></param>
        private static async Task EnqueueRedisMulti(this LinyeeWebSocketConnectionBase client, Func<LinyeeWebSocketConnectionBase, RedisCachingInfo> mf)
        {
            await VoidTask();
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
        /// <param name="sec"></param>
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

            //已有硬盘键
            if (item == null && keyitem != null)
            {
                return client.RedisSetDisk(keyitem, value, sec);
            }
            //回收硬盘键
            else if (item == null && delkeyitem != null)
            {
                return client.RedisSetDiskRestore(delkeyitem, value, sec);
            }
            //内存键
            else
            {
                //未有内存键 判断是否使用硬盘缓存
                if (item == null)
                {
                    if (RedisCaching.Count >= REDISProtocol.RedisCahingMaxCount)
                    {
                        return client.RedisSetDisk(key, value, sec);
                    }
                    if (CpuCount_Helper.GetCurMemUseRate() > REDISProtocol.RedisMaxMemUseRate)
                    {
                        return client.RedisSetDisk(key, value, sec);
                    }
                }

                //从内存转到硬盘 大于半值
                if ((RedisCaching.Count >= REDISProtocol.RedisCahingMaxCount || CpuCount_Helper.GetCurMemUseRate() > REDISProtocol.RedisMaxMemUseRate) && cachekey != null && cachekey.Length >= max2Len)
                {
                    RedisCaching.TryRemove(key, out item);
                    RedisCahingKeys.TryRemove(key, out cachekey);
                    cachekey.Key = "~" + DateTime.Now.Ticks + "~" + key;
                    cachekey.Status = 0;
                    RedisCahingKeys.TryAdd(cachekey.Key, cachekey);
                    return client.RedisSetDisk(key, value, sec);
                }

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

                CheckSave();//检查保存
            }

            return item;
        }

        /// <summary>
        /// 获取队列值
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
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
                RedisCaching.AddOrUpdate(delitem.Key, delitem,(k,v)=> {
                    v.State = EntityState.Deleted;
                    return v;
                });
            }

            var item= RedisCaching.Values.FirstOrDefault(p => p.Key == key);
            var keyitem = LinyeeWebSocketConnectionBase.RedisDiskKeys.Values.FirstOrDefault(p => p.Key == key);

            //if(item!=null) Console.WriteLine("内存表数据："+item.ToJsonString());
            //if (keyitem != null) Console.WriteLine("索引表数据：" + keyitem.ToJsonString());
            if (item == null && keyitem!=null)
            {
                return client.RedisGetDisk(keyitem);
            }
            else
            {
                //Console.WriteLine("索引表数据：" + LinyeeWebSocketConnectionBase.RedisDiskKeys.ToJsonString());
                return item;
            }
        }

        /// <summary>
        /// 获取值
        /// 硬盘数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static RedisCachingInfo RedisGetDisk(this LinyeeWebSocketConnectionBase client, RedisCachingKeyInfo key)
        {
            //Console.WriteLine("查找硬盘");

            if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);
            var DbValueFile = Path.Combine(DbPath, "dbdiskvalues.data");
            using (var valfile = new FileStream(DbValueFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024 * 16))
            {
                valfile.Position = key.Offset;
                using (var brval = new BinaryReader(valfile))
                {
                    var len = brval.ReadInt32();
                    var zipbuf = brval.ReadBytes(len);
                    var buf = zipbuf.FlaterDeCompress();
                    var json = buf.GetUTF8String();
                    var item = JsonConvert.DeserializeObject<RedisCachingInfo>(json);
                    return item;
                }
            }
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static async Task<RedisCachingInfo> RedisExpire(this LinyeeWebSocketConnectionBase client, string key, string value)
        {
            RedisCachingInfo item = null;
            long sec = 0;
            var b= long.TryParse(value, out sec);
            if (!b)
            {
                await client.SendRedisFAIL("过期时间不是一个有效的数值");
                return item;
            }

            return await client.RedisExpire(key, sec);
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="client"></param>
        /// <param name="key"></param>
        /// <param name="sec"></param>
        private static async Task<RedisCachingInfo> RedisExpire(this LinyeeWebSocketConnectionBase client, string key, long sec)
        {
            RedisCachingInfo item = null;
            if (sec <= 0)
            {
                await client.SendRedisFAIL("过期时间应于0");
                return item;
            }

            LinyeeWebSocketConnectionBase.RedisCaching.TryGetValue(key, out item);
            if (item == null)
            {
                await client.SendRedisFAIL("指定项不存在");
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


        #region Json协议

        /// <summary>
        /// Redis 登录
        /// </summary>
        /// <param name="client"></param>
        /// <param name="body"></param>
        /// <param name="RequestId"></param>
        public static async Task RedisLogin(this LinyeeWebSocketConnectionBase client, LoginRequest body, string RequestId)
        {
            var sign = body.GetSign();
            if (sign != body.sign)
            {
                await client.SendRedisFAIL("签名异常");
                return;
            }

            if ((DateTime.Now - body.CreateTime).TotalMinutes >= 10)
            {
                await client.CloseMsg(body.CreateTime.ToString("HH:mm:ss.ffffff") + " 时间误差过大，拒绝登录");
                return;
            }

            //检测登录
            var res = await client.MemberBll.CheckLoginAsync(body.loginAccount, body.DePassword,body.loginType);
            if (!res.IsOk)
            {
                await client.SendRedisFAIL(res.Msg);
            }
            else
            {
                client.LongId = res.Data.MemberId;
                client.Id = (int)client.LongId;
                client.Authed = true;

                await client.SendRedisBulk(await res.Data.ToJsonStringAsync());
            }
        }

        /// <summary>
        /// Redis 操作
        /// </summary>
        /// <param name="client"></param>
        /// <param name="mqmsg"></param>
        /// <param name="revmsg"></param>
        /// <returns></returns>
        public static async Task RunRedis(this LinyeeWebSocketConnectionBase client, MQProtocol mqmsg, string revmsg)
        {
            var mqQuest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase>>(revmsg);

            switch (mqmsg.Command)
            {
                #region "缓存 key-value"
                case MQProtocolCommandType.MULTI:
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
                case MQProtocolCommandType.SET:
                    {
                        var mqquest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase<RedisSetRequest>>>(revmsg);
                        var body = mqquest.Data.Body;
                        var key = body.Key;
                        var val = body.Value;
                        var sec = body.ExpireSec;

                        //检查键名
                        if (!await client.CheckRedisKey(key))
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


                        if (sec < 0)
                        {
                            await client.SendRedisFAIL("过期时间不是一个有效的数值"+ msg);
                            return ;
                        }

                        if (client.IsRedisMulti)
                        {
                            await client.EnqueueRedisMulti((c) => c.RedisSet(key, val, sec));
                            await client.SendRedisOK(msg);
                        }
                        else
                        {
                            var res = client.RedisSet(key, val, sec);
                            if (res != null) await client.SendRedisOK(msg);
                            else await client.SendRedisFAIL(msg);
                        }
                    }
                    break;
                case MQProtocolCommandType.GET:
                    {
                        var mqquest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase<RedisGetRequest>>>(revmsg);
                        var body = mqquest.Data.Body;

                        var res = client.RedisGet(body.Key);
                        if (res != null) await client.SendRedisBulk(res.GetValue());
                        else await client.SendRedisBulk();
                    }
                    break;
                case MQProtocolCommandType.EXPIRE:
                    {
                        var mqquest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase<RedisExpireRequest>>>(revmsg);
                        var body = mqquest.Data.Body;
                        if (client.IsRedisMulti)
                        {
                            await client.EnqueueRedisMulti((c) =>c.RedisExpire(body.Key, body.ExpireSec).Result);
                        }
                        else
                        {
                            var res = client.RedisExpire(body.Key, body.ExpireSec);
                            if (res != null) await client.SendRedisOK();
                            else await client.SendRedisFAIL();
                        }
                    }
                    break;
                case MQProtocolCommandType.EXEC:
                    {
                        await client.RedisMultiEXEC();
                    }
                    break;
                case MQProtocolCommandType.DISCARD:
                    {
                        client.IsRedisMulti = false;
                    }
                    break;
                case MQProtocolCommandType.EXISTS:
                    {
                        var mqquest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase<RedisGetRequest>>>(revmsg);
                        var body = mqquest.Data.Body;
                        await client.RedisExists(body.Key);
                    }
                    break;
                case MQProtocolCommandType.EVAL:
                    {
                        var mqquest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase<RedisGetRequest>>>(revmsg);
                        var input = mqquest.Data.Body.Key;
                        var rr = new RedisRepalce(client, input);
                        var text = rr.valText;
                        var res = rr.result;
                        if (res.IsOk)
                        {
                            var val = text.eval();
                            if (val.Type == EvalObjectType.Error)
                            {
                                await client.SendRedisFAIL(val.Value?.ToString());
                            }
                            else
                            {
                                await client.SendRedisInteger(val.Value);
                            }
                        }
                        else
                        {
                            await client.SendRedisFAIL(res.Msg);
                        }
                    }
                    break;
                #endregion

                #region "服务端"
                case MQProtocolCommandType.AUTH:
                    {
                        var mqquest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase<LoginRequest>>>(revmsg);
                        LogService.AnyLog("ReQuestSponse", "APP端登录参数：", revmsg);
                        var body = mqquest.Data.Body;

                        await client.RedisLogin(body, mqquest.Data.RequestId);
                    }
                    break;
                case MQProtocolCommandType.PING:
                    {
                        await client.SendRedisOK("PONG");
                    }
                    break;
                case MQProtocolCommandType.SAVE:
                    {
                        await client.RedisSave();
                    }
                    break;
                #endregion

                #region "队列 List"
                case MQProtocolCommandType.ENQ:
                case MQProtocolCommandType.LPUSH:
                    {
                        var mqquest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase<RedisSetRequest>>>(revmsg);
                        var body = mqquest.Data.Body;
                        await client.RedisExists(body.Key);

                        var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
                        var key = body.Key;
                        var val = body.Value;

                        Queue<string> que = new Queue<string>();
                        if (!RedisQueue.ContainsKey(key)) RedisQueue.TryAdd(key, que);
                        else que = RedisQueue[key];
                        que.Enqueue(val);
                        await client.SendRedisOK();
                        return;
                    }
                case MQProtocolCommandType.DEQ:
                case MQProtocolCommandType.LPOP:
                    {
                        var mqquest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase<RedisGetRequest>>>(revmsg);
                        var body = mqquest.Data.Body;

                        var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
                        var key = body.Key;
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
                case MQProtocolCommandType.PEEKQ:
                    {
                        var mqquest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase<RedisGetRequest>>>(revmsg);
                        var body = mqquest.Data.Body;

                        var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
                        var key = body.Key;
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
                case MQProtocolCommandType.LENQ:
                case MQProtocolCommandType.LLEN:
                    {
                        var mqquest = JsonConvert.DeserializeObject<MQProtocol<MQReQuestBase<RedisGetRequest>>>(revmsg);
                        var body = mqquest.Data.Body;

                        var RedisQueue = LinyeeWebSocketConnectionBase.RedisQueue;
                        var key = body.Key;
                        Queue<string> que = new Queue<string>();
                        if (!RedisQueue.ContainsKey(key)) RedisQueue.TryAdd(key, que);
                        else que = RedisQueue[key];

                        var val = que.Count;
                        await client.SendRedisBulk(val.ToString());
                    }
                    break;
                #endregion

                default:
                    {
                        await client.SendRedisFAIL("不支持的命令"+ mqmsg.Command.ToString());
                    }
                    break;
            }
        }
        #endregion
    }
}

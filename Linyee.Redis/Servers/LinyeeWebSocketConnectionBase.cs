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
using WS_Server.Servers;
using WS_Server.SocketServers;

namespace WS_Server.Servers
{
    /// <summary>
    /// WebSokcet连接信息
    /// Linyee 2018-06-03
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public partial class LinyeeWebSocketConnectionBase : LinyeeWebSocketConnectionBaseInfo,IDisposable
    {
        #region "定义"
        #region     "全局"

        /// <summary>
        /// 多线程字典 连接
        /// </summary>
        public readonly static ConcurrentDictionary<Guid, LinyeeWebSocketConnectionBase> OnlineSockets = new ConcurrentDictionary<Guid, LinyeeWebSocketConnectionBase>();

        /// <summary>
        /// 多线程字典 Redis 缓存
        /// </summary>
        public readonly static ConcurrentDictionary<string, RedisCachingInfo> RedisCaching = new ConcurrentDictionary<string, RedisCachingInfo>();

        /// <summary>
        /// 多线程字典 Redis 内存缓存键
        /// </summary>
        public readonly static ConcurrentDictionary<string, RedisCachingKeyInfo> RedisCahingKeys = new ConcurrentDictionary<string, RedisCachingKeyInfo>();

        /// <summary>
        /// 多线程字典 Redis 硬盘缓存键
        /// </summary>
        public readonly static ConcurrentDictionary<string,RedisCachingKeyInfo> RedisDiskKeys = new ConcurrentDictionary<string, RedisCachingKeyInfo>();

        /// <summary>
        /// 多线程字典 Redis 事务
        /// </summary>
        public readonly static ConcurrentDictionary<string, RedisMultiInfo> RedisMulti = new ConcurrentDictionary<string, RedisMultiInfo>();

        /// <summary>
        /// 多线程字典 Redis 队列
        /// </summary>
        public readonly static ConcurrentDictionary<string, Queue<string>> RedisQueue = new ConcurrentDictionary<string, Queue<string>>();

        /// <summary>
        /// 多线程字典 Redis 队列消息
        /// 普通主题以/开头
        /// 保存的主题以Save开头
        /// </summary>
        public readonly static ConcurrentDictionary<string, Queue<QueueMessage>> RedisQueueMessage = new ConcurrentDictionary<string, Queue<QueueMessage>>();

        /// <summary>
        /// 多线程字典 Redis 主题
        /// </summary>
        public readonly static ConcurrentDictionary<string, RedisTopicInfo> RedisTopic = new ConcurrentDictionary<string, RedisTopicInfo>();

        /// <summary>
        /// 当前Redis最后操作时间
        /// </summary>
        [JsonIgnore]
        internal static DateTime RedisOperTime=DateTime.MinValue;

        /// <summary>
        /// 当前Redis最后保存时间
        /// </summary>
        [JsonIgnore]
        internal static DateTime RedisSaveTime = DateTime.MinValue;

        /// <summary>
        /// 当前Redis操作次数
        /// </summary>
        [JsonIgnore]
        internal static int RedisOperCount;
        #endregion

        #region 实例
        /// <summary>
        /// 最后请求Id
        /// </summary>
        [JsonIgnore]
        public string LastRequestId = "";
        /// <summary>
        /// 多线程字典 响应数据消息
        /// </summary>
        [JsonIgnore]
        public readonly ConcurrentDictionary<string, MQProtocol<MQReSponseBase<PayResponse>>> ResponseMsg = new ConcurrentDictionary<string, MQProtocol<MQReSponseBase<PayResponse>>>();
        #endregion

        /// <summary>
        /// 用户登录服务 注入
        /// </summary>
        internal IMemberBll MemberBll = null;
        #endregion

        #region "恢复Redis 持久化数据"

        /// <summary>
        /// 持久化 恢复
        /// </summary>
        static LinyeeWebSocketConnectionBase()
        {
            ReadRedisCahingKeys();
            ReadRedisDb();

            RedisOperTime = DateTime.Now;
            RedisSaveTime = DateTime.Now;
            TimerService.Default.Minute5 += Default_Minute5;

            ReadRedisDiskKeys();
            //读取主题
            ReadRedisTopic();
        }

        /// <summary>
        /// 读取主题
        /// </summary>
        private static void ReadRedisTopic()
        {
            var filename = "dbtopics.data";
            var dictname = "主题";
            ReadRedisTopic(RedisTopic, filename, dictname);
        }

        /// <summary>
        /// 读取主题
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="filename"></param>
        /// <param name="dictname"></param>
        private static void ReadRedisTopic(ConcurrentDictionary<string, RedisTopicInfo> dict, string filename, string dictname)
        {
            var DbValueFile = Path.Combine(DbPath, filename);
            if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);

            using (var valfile = new FileStream(DbValueFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 1024 * 1024 * 16))
            {
                if (valfile.Length > 0)
                {
                    using (var sr = new StreamReader(valfile, Encoding.UTF8))
                    {
                        var json = sr.ReadToEnd();
                        if (string.IsNullOrEmpty(json)) json = "{}";
                        var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RedisTopicInfo>>(json);
                        foreach(var item in list)
                        {
                            dict.TryAdd(item.Topic, item);
                        }
                    }
                }
            }
            string msg = dictname + "数据加载完成，共" + dict.Count + "个数据";
            Console.WriteLine(DateTime.Now.ToString("dd HH:mm:ss") + "\t" + msg);
            LogService.Runtime(msg);
        }

        /// <summary>
        /// 数据所在目录
        /// </summary>
        internal readonly static string DbPath = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\RedisDb\";

        /// <summary>
        /// 持久化 恢复 硬盘数据索引
        /// </summary>
        private static void ReadRedisDiskKeys()
        {
            var filename = "dbdiskkeys.data";
            var dictame = "硬盘";
            ReadRedisKeys(RedisDiskKeys, filename, dictame);
        }

        /// <summary>
        /// 持久化 恢复 内存数据索引
        /// </summary>
        private static void ReadRedisCahingKeys()
        {
            var filename = "dbkeys.data";
            var dictame = "内存";
            ReadRedisKeys(RedisCahingKeys,filename,dictame);
        }

        /// <summary>
        /// 持久化 恢复 内存数据索引
        /// </summary>
        private static void ReadRedisKeys(ConcurrentDictionary<string, RedisCachingKeyInfo> dict, string filename,string dictname)
        {
            var DbValueFile = Path.Combine(DbPath, filename);
            if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);

            using (var valfile = new FileStream(DbValueFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 1024 * 1024 * 16))
            {
                if (valfile.Length > 0)
                {
                    using (var br = new BinaryReader(valfile, Encoding.UTF8))
                    {
                        while (valfile.Position >= 0 && valfile.Position < valfile.Length)
                        {
                            var Key = br.ReadLine();
                            var Offset = br.ReadInt64();
                            var Len = br.ReadInt64();
                            var Status = br.ReadByte();

                            var item = new RedisCachingKeyInfo(Key, Offset, Len, Status);
                            dict.TryAdd(item.Key, item);
                        }
                    }
                }
                //else
                //{
                //    Console.WriteLine("{0} {1} {2}", dictname, filename,"为空");
                //}
            }
            string msg = dictname+ "索引数据加载完成，共" + dict.Count + "个索引";
            Console.WriteLine(DateTime.Now.ToString("dd HH:mm:ss") + "\t" + msg);
            LogService.Runtime(msg);
            //Console.WriteLine("内存索引表数据：" + RedisCahingKeys.ToJsonString());
        }

        /// <summary>
        /// 持久化 恢复 内存数据
        /// </summary>
        private static void ReadRedisDb()
        {
            var DbValueFile = Path.Combine(DbPath, "dbvalue.data");
            if (!Directory.Exists(DbPath)) Directory.CreateDirectory(DbPath);

            using (var valfile = new FileStream(DbValueFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 1024 * 1024 * 16))
            {
                if (valfile.Length > 0)
                {
                    using (var br = new BinaryReader(valfile))
                    {
                        foreach (var key in RedisCahingKeys.Values.Where(p => REDISProtocol.FirstChars.Contains(p.Key.ElementAt(0)) && p.Status>0).OrderBy(p => p.Offset))
                        //while (!sr.EndOfStream)
                        {
                            valfile.Position = key.Offset;
                            var len = br.ReadInt32();
                            var Flaterbuf = br.ReadBytes(len);
                            var buf = Flaterbuf.FlaterDeCompress();
                            var json = Encoding.UTF8.GetString(buf);
                            RedisCachingInfo tmpval = JsonConvert.DeserializeObject<RedisCachingInfo>(json);
                            LinyeeWebSocketConnectionBase.RedisCaching.TryAdd(tmpval.Key, tmpval);
                        }
                    }
                }
            }
            string msg = "数据加载完成，共" + RedisCaching.Count + "个数据";
            Console.WriteLine(DateTime.Now.ToString("dd HH:mm:ss") + "\t"+ msg);
            LogService.Runtime(msg);
        }

        /// <summary>
        /// 5分钟执行一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static async void Default_Minute5(object sender, TimerService.LinyeeTimerEventArgs e)
        {
            LogService.Runtime("定时任务");
            if ((RedisOperTime- RedisSaveTime).TotalMinutes >= 5)
            {
                LogService.Runtime("开始保存");
                await LinyeeWebSocketConnectionBase_Redis.RedisSave();
                LogService.Runtime("结束保存");
            }
            else
            {
                LogService.Runtime("无需保存");
            }
        }
        #endregion

        #region "判断"
        /// <summary>
        /// 是否 在线集中
        /// </summary>
        /// <returns></returns>
        internal bool InOnlie()
        {
            return OnlineSockets.Values.Any(p => p.ClientId == this.ClientId);
        }
        #endregion

        #region "移除连接"
        /// <summary>
        /// 移除连接
        /// </summary>
        public static void TryRemove(Guid ClientId)
        {
            if (OnlineSockets.ContainsKey(ClientId))
            {
                OnlineSockets[ClientId]?.Dispose();
            }
        }
        #endregion

        #region "IDisposable"

        /// <summary>
        /// 发送消息后关闭连接并释放资源
        /// </summary>
        /// <param name="msg"></param>
        public  async Task CloseMsg(string msg = "正常关闭")
        {
            LogService.WebSocket10Minute(LongId.ToString(),ClientId.ToString(), "关闭消息：", msg);
            //await this.SendMsg(msg, messageType: WebSocketMessageType.Close);
            try
            {
                await Client.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, CancelToken);
            }
            catch (Exception ex)
            {
                LogService.Exception(ex);
            }
            //await Client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, msg, CancelToken);
            //Client.Abort();
            this.Dispose();
        }

        /// <summary>
        /// 释放资源 针对已断开连接
        /// </summary>
        public void Dispose()
        {
            LogService.WebSocket10Minute(LongId.ToString(), "关闭服务：", ClientId.ToString());
            if (this.TypeCode == 0)//匿名时移除所有关注的主题
            {
                LogService.WebSocket10Minute(LongId.ToString(), "移除所有关注的主题：", ClientId.ToString());
                RedisTopic.RemoveSub(this);
            }
            LinyeeWebSocketConnectionBase obj = null;
            OnlineSockets.TryRemove(this.ClientId, out obj);
            this.Closed = true;
            this.Client.Dispose();
        }
        #endregion

        #region "构造"
        /// <summary>
        /// 创建默认连接对象
        /// </summary>
        public LinyeeWebSocketConnectionBase()
        {
            this.ConnectedTime = DateTime.Now;
        }
        /// <summary>
        /// 创建默认连接对象
        /// </summary>
        public LinyeeWebSocketConnectionBase(IMemberBll imb) :this()
        {
            this.MemberBll = imb;
        }

        /// <summary>
        /// 克隆一个副本，不含计时器
        /// </summary>
        /// <returns></returns>
        internal virtual LinyeeWebSocketConnectionBase Clone()
        {
            LinyeeWebSocketConnectionBase obj = new LinyeeWebSocketConnectionBase(MemberBll)
            {
                Client = this.Client,
                ClientId = this.ClientId,
                LongId = this.LongId,
                Id = this.Id,
                Name = this.Name,
                TypeCode = this.TypeCode,

                ConnectedTime = this.ConnectedTime,
                CancelToken=this.CancelToken,
                LastMsgTime = this.LastMsgTime,
            };
            return obj;
        }

        #endregion

        #region "属性"

        /// <summary>
        /// 是否已关闭
        /// </summary>
        [JsonIgnore]
        public bool Closed { get; internal set; } = false;

        /// <summary>
        /// 登录账号
        /// </summary>
        //[JsonIgnore]
        public string loginAccount { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        [JsonIgnore]
        public string loginPassword { get; set; }

        /// <summary>
        /// 长ID
        /// 设置值时必须在TypeCode之后
        /// </summary>
        public new long LongId {
            get { return base.LongId; }
            protected internal set
            {
                if (value > 0)
                {
                    var items= OnlineSockets.Values.Where(p => p.LongId == value && p.TypeCode == MainTypeCode).ToList();
                    foreach(var item in items)
                    {
                        var res= item.CloseMsg("您已在其它地方登录");
                    }
                }
                base.LongId = value;
            }
        }

        /// <summary>
        /// 放弃操作的令牌
        /// </summary>
        [JsonIgnore]
        public CancellationToken CancelToken { get; protected set; }

        /// <summary>
        /// 客户端
        /// </summary>
        [JsonIgnore]
        public WebSocket Client { get; protected set; }

        /// <summary>
        /// 代理端
        /// </summary>
        [JsonIgnore]
        public Socket Agent { get { if (p_Agent == null) p_Agent = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); return p_Agent; } }
        private Socket p_Agent;

        /// <summary>
        /// 是否事务
        /// </summary>
        [JsonIgnore]
        public bool IsRedisMulti { get {
                var RedisMulti = LinyeeWebSocketConnectionBase.RedisMulti;
                return RedisMulti.Values.Any(p => p.LongId == LongId && p.MainTypeCode == MainTypeCode);
            }
            internal set {
                var RedisMulti = LinyeeWebSocketConnectionBase.RedisMulti;
                if (value)
                {
                    var b = RedisMulti.Values.Any(p => p.LongId == LongId && p.MainTypeCode == MainTypeCode);
                    if (!b)
                    {
                        var item = new RedisMultiInfo(LongId, MainTypeCode);
                        RedisMulti.TryAdd(item.Key, item);
                    }
                }
                else
                {
                    this.RedisMultiClear();
                }
            }
        }

        #endregion
    }
}

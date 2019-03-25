using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using WS_Core.Consts;
using WS_Core.Enums;

namespace WS_Core.Bodys
{
    /// <summary>
    /// RedisCaching 键信息
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public class RedisCachingKeyInfo
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key;

        /// <summary>
        /// 偏移
        /// </summary>
        public long Offset=-1;

        /// <summary>
        /// 字符长度
        /// </summary>
        public long Length;

        /// <summary>
        /// 记录状态 1正常 0删除（目前使用特殊符号开始的键名作为删除组） 2限用(相同MainTypeCode) 3限用（权限描述）
        /// </summary>
        public byte Status=1;
        #region "预留 未持久化到硬盘"

        /// <summary>
        /// 主类别
        /// </summary>
        public byte MainTypeCode = 0;

        /// <summary>
        /// 权限描述位 表示2的幂位
        /// </summary>
        public byte permissions = 0;
        #endregion

        /// <summary>
        /// 关系状态 0未关联 1未变更 2添加的 3修改的 4删除的
        /// 无需持久化
        /// </summary>
        public byte State = 1;

        /// <summary>
        /// 键信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        public RedisCachingKeyInfo(string key, long offset, long len)
        {
            Key = key;
            Offset = offset;
            Length = len;
        }

        /// <summary>
        /// 键信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <param name="status"></param>
        public RedisCachingKeyInfo(string key, long offset, long len, byte status) : this(key, offset, len)
        {
            Status = status;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class RedisCachingInfo_Ex
    {

        /// <summary>
        /// 设置值，默认是已经压缩过的
        /// </summary>
        /// <param name="info"></param>
        /// <param name="value"></param>
        /// <param name="hasgzip"></param>
        /// <returns></returns>
        public static RedisCachingInfo SetValue(this RedisCachingInfo info, string value, bool hasgzip = true)
        {
            if (hasgzip)
            {
                info._Value = value;
                info.HasGzip = hasgzip;
            }
            else
            {
                info.Value = value;
            }
            return info;
        }

        /// <summary>
        /// 获取值，自适应压缩原生
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string GetValue(this RedisCachingInfo info)
        {
            if (info.HasGzip)
            {
                //return info._Value.GZipDecompressString();
                return info._Value.FlaterDeCompressString();
            }
            else
            {
                return info._Value;
            }
        }

    }

    /// <summary>
    /// Redis缓存数据格式
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public class RedisCachingInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public RedisCachingInfo()
        { }

        /// <summary>
        /// 永久有效
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public RedisCachingInfo(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// 指定有效时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="sec"></param>
        public RedisCachingInfo(string key, string value, long sec):this(key,value)
        {
            ExpireSec = sec;

            if (ExpireSec > 0)
            {
                ExpireTime = DateTime.Now.AddSeconds(ExpireSec);
            }
        }

        /// <summary>
        /// 值
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 值
        /// 长度等于<seealso cref="REDISProtocol.ZipStartLength"/>开始压缩
        /// 压缩长度无优势时使用原值
        /// </summary>
        public string Value { get {
                return _Value;
            }
            set {
                if (value.Length >= REDISProtocol.ZipStartLength)
                {
                    //var gziped = value.GZipCompressString();
                    var gziped = value.FlaterCompressString();
                    if(gziped.Length< value.Length)
                    {
                        HasGzip = true;
                        _Value = gziped;
                    }
                    else
                    {
                        HasGzip = false;
                        _Value = value;
                    }
                }
                else
                {
                    HasGzip = false;
                    _Value = value;
                }
            }
        }
        internal string _Value;
        /// <summary>
        /// 是否已压缩
        /// </summary>
        [JsonIgnore]
        public bool HasFlater => HasGzip;

        /// <summary>
        /// 是否已压缩
        /// </summary>
        public bool HasGzip { get; set; }

        /// <summary>
        /// 过期秒数
        /// 默认0 永不失效
        /// </summary>
        public long ExpireSec { get; set; } = 0;

        /// <summary>
        /// 状态
        /// EntityState.Detached 分离
        /// </summary>
        [JsonIgnore]
        public EntityState State { get; set; } = EntityState.Unchanged;

        /// <summary>
        /// 过期时间
        /// 默认 最大值 永不失效
        /// </summary>
        public DateTime ExpireTime { get; set; } = DateTime.MaxValue;
    }


    /// <summary>
    /// Redis子命令枚举
    /// </summary>
    [Author("Linyee", "2019-03-19")]
    public enum RedisSubCommand
    {
        /// <summary>
        /// 设置名称
        /// </summary>
        SETNAME,//
        /// <summary>
        /// 获取对象
        /// </summary>
        GET,//
    }


    /// <summary>
    /// Redis命令枚举
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public enum RedisCommand
    {
        #region "字符串 缓存"
        /// <summary>
        /// 
        /// </summary>
        GET, //获取一个key的值
        /// <summary>
        /// 
        /// </summary>
        INFO, //Redis信息。  
              /// <summary>
              /// 
              /// </summary>
        SET, //添加一个值
        /// <summary>
        /// 
        /// </summary>
        EXPIRE, //设置过期时间
                /// <summary>
                /// 
                /// </summary>
        MULTI, //标记一个事务块开始
        /// <summary>
        /// 
        /// </summary>
        EXEC, //执行所有 MULTI 之后发的命令

        /// <summary>
        /// 
        /// </summary>
        MGET,//获取多个key
             /// <summary>
             /// 
             /// </summary>
        MSET,//设置多个key

        /// <summary>
        /// 取消事务 中止事务
        /// </summary>
        DISCARD,//取消事务 中止事务
        #endregion

        #region "队列消息 主题消息"
        /// <summary>
        /// 
        /// </summary>
        SUBSCRIBE,//订阅给定的一个或多个频道的信息
                  /// <summary>
                  /// 
                  /// </summary>
        UNSUBSCRIBE,//指退订给定的频道
                    /// <summary>
                    /// 
                    /// </summary>
        PSUBSCRIBE,//订阅一个或多个符合给定模式的频道
        /// <summary>
        /// 
        /// </summary>
        PUNSUBSCRIBE,//退订所有给定模式的频道
                     /// <summary>
                     /// 
                     /// </summary>
        PUBLISH,//将信息发送到指定的频道
                /// <summary>
                /// 
                /// </summary>
        PUBSUB,//查看订阅与发布系统状态
        #endregion

        #region "服务端命令"
        /// <summary>
        /// 
        /// </summary>
        DBSIZE,
        /// <summary>
        /// 
        /// </summary>
        DEL,
        /// <summary>
        /// 
        /// </summary>
        SELECT,
        /// <summary>
        /// 
        /// </summary>
        AUTH,//认证 登录
             /// <summary>
             /// 
             /// </summary>
        PING,//ping
             /// <summary>
             /// 
             /// </summary>
        EXISTS,//是否存在
               /// <summary>
               /// 
               /// </summary>
        QUIT,//退出
             /// <summary>
             /// 
             /// </summary>
        SAVE,//主动保存
             /// <summary>
             /// 
             /// </summary>
        EVAL,//计算表达式
        #endregion

        #region "Hash"
        /// <summary>
        /// 
        /// </summary>
        HSET,
        /// <summary>
        /// 
        /// </summary>
        HGET,
        #endregion

        #region "集合"
        /// <summary>
        /// 
        /// </summary>
        SADD,//添加元素 sadd key value1 value2
             /// <summary>
             /// 
             /// </summary>
        SREM,//删除元素
             /// <summary>
             /// 
             /// </summary>
        SPOP,//随机获取一个值并删除它
        #endregion

        #region "有序集合"
        /// <summary>
        /// 
        /// </summary>
        ZADD,//添加元素 sadd key value1 value2
             /// <summary>
             /// 
             /// </summary>
        ZREM,//删除元素
             /// <summary>
             /// 
             /// </summary>
        ZPOP,//随机获取一个值并删除它
        #endregion

        #region "队列 List"
        /// <summary>
        /// 
        /// </summary>
        LPUSH,//压入
        /// <summary>
        /// 
        /// </summary>
        LPOP,//弹出
        /// <summary>
        /// 
        /// </summary>
        LLEN,//当前长度
        #endregion

        #region "队列 Queue"
        /// <summary>
        /// 
        /// </summary>
        ENQ,//压入
            /// <summary>
            /// 
            /// </summary>
        DEQ,//弹出
        /// <summary>
        /// 
        /// </summary>
        PEEKQ,//检查最顶元素
        /// <summary>
        /// 
        /// </summary>
        LENQ,//当前长度
             /// <summary>
             /// 
             /// </summary>
        CLIENT,
        /// <summary>
        /// 
        /// </summary>
        CONFIG,
        /// <summary>
        /// 
        /// </summary>
        ECHO,
        /// <summary>
        /// 
        /// </summary>
        REDISAGENT,
        /// <summary>
        /// 
        /// </summary>
        CLUSTER
        #endregion

    }

    //public void 容器时间复杂度平均()
    //{
    //    //Dictionary<int, object> 字典; //字典是最快的
    //    //访问.O(1); 搜索.O(1); 插入.O(1); 删除.O(1);
    //    //Hashtable 哈希表; HashSet<int> 哈希集;
    //    //访问.O(null); 搜索.O(1); 插入.O(1); 删除.O(1);
    //    //SortedList<int, object> 有序列表;
    //    //访问.O(1); 搜索.O(Log(n)); 插入.O(Log(n)); 删除.O(n);
    //    //SortedDictionary<int, object> 有序字典;
    //    //访问.O(1); 搜索.O(Log(n)); 插入.O(Log(n)); 删除.O(Log(n));
    //    //Queue 队列; Stack 堆栈; LinkedList<int> 双向链表;
    //    //访问.O(n); 搜索.O(n); 插入.O(1); 删除.O(1);
    //    //Array 数组; List<int> 列表;
    //    //访问.O(1); 搜索.O(n); 插入.O(n); 删除.O(n);
    //    //SortedSet<int> 有序集; //二叉树
    //    //访问.O(Log(n)); 搜索.O(Log(n)); 插入.O(Log(n)); 删除.O(Log(n));
    //}

    /// <summary>
    /// Redis 
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public class REDISProtocol
    {
        #region 配置
        /// <summary>
        /// 开始压缩的字符长度
        /// </summary>
        public static int ZipStartLength = 128;

        /// <summary>
        /// 最大操作次数
        /// 达标时，自动保存，或自动执行事务
        /// </summary>
        public static int RedisOperMaxCount = 1000;

        /// <summary>
        /// 最大内存使用率
        /// 达标时，自动转为硬盘模式，或自动执行事务
        /// </summary>
        public static uint RedisMaxMemUseRate = 80;

        /// <summary>
        /// 最大内存表记录数
        /// 达标时，自动转为硬盘模式
        /// </summary>
        public static int RedisCahingMaxCount = 1000000;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public static string OKString = "+OK\r\n";

        /// <summary>
        /// 键名不允许的字符
        /// </summary>
        public static Regex keynochars = new Regex("[\\+\\/\\\\@~!#$%^&*()=\\[\\]{};:'\"|?.,\\>\\<]+", RegexOptions.Compiled);
        /// <summary>
        /// 分行
        /// </summary>
        internal static Regex newline = new Regex("\n+|(\r\n)+", RegexOptions.Compiled);
        /// <summary>
        /// 标准化
        /// </summary>
        internal static Regex normalize = new Regex("($\\d+)(\n+|(\r\n)+)", RegexOptions.Compiled);
        /// <summary>
        /// 子分行
        /// </summary>
        internal static Regex subline = new Regex("\x1A1310", RegexOptions.Compiled);
        /// <summary>
        /// 首字母字符集不含删除符
        /// </summary>
        public readonly static string FirstChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        /// <summary>
        /// 首字母字符集含删除符
        /// </summary>
        public readonly static string FirstCharsDel= FirstChars+"~";

        /// <summary>
        /// 创建Redis动作
        /// </summary>
        /// <param name="text"></param>
        public static REDISAction CreateREDISAction(string text)
        {
            var revmsg = normalize.Replace(text, "$1\x1A1310");//标准化
            var c = revmsg.ToCharArray()[0];
            var content = revmsg.Substring(1);
            var lines = newline.Split(content);
            REDISAction result = new REDISAction(c, lines);
            return result;
        }

        /// <summary>
        /// 创建Redis客户端命令动作
        /// </summary>
        /// <param name="text"></param>
        public static REDISClientCommand CreateREDISClientCommand(string text)
        {
            var revmsg = normalize.Replace(text, "$1\x1A1310");//标准化
            var c = revmsg.ToCharArray()[0];
            var content = revmsg.Substring(1);
            var lines = newline.Split(content);
            REDISClientCommand result = new REDISClientCommand(c, lines);
            return result;
        }

        /// <summary>
        /// 创建Redis客户端命令动作
        /// </summary>
        /// <param name="sr"></param>
        public static REDISClientCommand CreateREDISClientCommand(StreamReader sr)
        {
            REDISClientCommand result = new REDISClientCommand(sr);
            return result;
        }

        /// <summary>
        /// 创建Redis簇信息
        /// </summary>
        /// <param name="revmsg"></param>
        public static REDISBulk CreateREDISBulk(string revmsg)
        {
            var c =revmsg.ToCharArray()[0];
            var content = revmsg.Substring(1);
            var lines = subline.Split(content);
            REDISBulk result = new REDISBulk(c, lines);
            return result;
        }

        /// <summary>
        /// 创建Redis簇信息
        /// </summary>
        /// <param name="sr"></param>
        public static REDISBulk CreateREDISBulk(StreamReader sr)
        {
            REDISBulk result = new REDISBulk(sr);
            return result;
        }
    }


    /// <summary>
    /// Redis命令
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public class REDISCommand: REDISAction<string>
    {
        /// <summary>
        /// 参数
        /// </summary>
        public string[] args { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public static explicit operator REDISCommand(REDISAction obj)
        {
            var item= new REDISCommand()
            {
                action = obj.action,
                body = obj.body.ToString(),
                args = obj.body.ToString().Trim(' ').Split(' '),
            };
            item.count = item.args.Length;
            return item;
        }
    }


    /// <summary>
    /// Rdeis动作
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public class REDISAction : REDISAction<object>
    {
        /// <summary>
        /// 子分行
        /// </summary>
        private static Regex subline = new Regex("\x1A1310", RegexOptions.Compiled);

        /// <summary>
        /// 内容
        /// </summary>
        public new object body { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public REDISAction() { }

        /// <summary>
        /// Rdeis动作
        /// </summary>
        /// <param name="c"></param>
        /// <param name="lines"></param>
        public REDISAction(char c, string[] lines)
        {
            action = c;
            switch (c)
            {
                case REDIS.Simple:
                    body = lines[0];
                    break;
                case REDIS.Errors:
                    body = lines[0];
                    break;
                case REDIS.Integer:
                    body = int.Parse(lines[0]);
                    break;
                case REDIS.Bulk:
                    var sublines = subline.Split(lines[0]);
                    count = int.Parse(sublines[0]);
                    if (count == -1) body = null;
                    else body = sublines[1];
                    break;
                case REDIS.Arrays:
                    count = int.Parse(lines[0]);
                    if (count > 0)
                    {
                        for (var fi = 1; fi < lines.Length; fi++)
                        {
                            list.Add(REDISProtocol.CreateREDISAction(lines[fi]));
                        }
                    }
                    body = list;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Rdeis 动作
        /// </summary>
        /// <param name="sr"></param>
        public REDISAction(StreamReader sr)
        {
            this.action = (char)sr.Read();
            if (!REDIS.Headers.Contains(""+action)) throw new Exception("动作头错误");
            switch (action)
            {
                case REDIS.Simple:
                    body = sr.ReadLine();
                    break;
                case REDIS.Errors:
                    body = sr.ReadLine();
                    break;
                case REDIS.Integer:
                    var val = sr.ReadLine();
                    //if (val.IndexOf(".") >= 0) body = double.Parse(val);
                    //else body = long.Parse(val);
                    if (val.IndexOf(".") >= 0) body = BigDecimal.Parse(val);
                    else body = BigInteger.Parse(val);
                    break;
                case REDIS.Bulk:
                    count = int.Parse(sr.ReadLine());
                    body = sr.ReadLine();
                    break;
                case REDIS.Arrays:
                    count = int.Parse(sr.ReadLine());
                    if (count > 0)
                    {
                        var list = new List<REDISAction>();
                        for (var fi = 0; fi < count; fi++)
                        {
                            list.Add(new REDISAction(sr));
                        }
                        body = list;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    #region "命令"

    /// <summary>
    /// Rdeis 客户端命令
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public class REDISClientCommand : REDISAction<List<REDISBulk>>
    {
        /// <summary>
        /// 
        /// </summary>
        public REDISClientCommand() { }

        /// <summary>
        /// Rdeis 客户端命令
        /// </summary>
        /// <param name="sr"></param>
        public REDISClientCommand(StreamReader sr)
        {
            this.action = (char)sr.Read();
            if (action != REDIS.Arrays) throw new Exception("命令头错误");
            count = int.Parse(sr.ReadLine());
            if (count > 0)
            {
                body = new List<REDISBulk>();
                for (var fi = 0; fi < count; fi++)
                {
                    body.Add(new REDISBulk(sr));
                }
            }
        }

        /// <summary>
        /// Rdeis 客户端命令
        /// </summary>
        /// <param name="br"></param>
        public REDISClientCommand(BinaryReader br)
        {
            var ch= (char)br.ReadByte();
            this.action = ch;
            if (action != REDIS.Arrays) throw new Exception(br.BaseStream.Position + "命令头错误"+ action + ((int)action).ToString());
            count = int.Parse(br.ReadLine2());
            if (count > 0)
            {
                body = new List<REDISBulk>();
                for (var fi = 0; fi < count; fi++)
                {
                    if (br.BaseStream.Position >= br.BaseStream.Length) break;
                    body.Add(new REDISBulk(br));
                }
            }
        }

        /// <summary>
        /// Rdeis动作
        /// </summary>
        /// <param name="c"></param>
        /// <param name="lines"></param>
        public REDISClientCommand(char c, string[] lines)
        {
            action = c;
            if (c != REDIS.Arrays) throw new Exception("命令头错误");
            count = int.Parse(lines[0]);
            if (count > 0)
            {
                body = new List<REDISBulk>();
                for (var fi = 1; fi < lines.Length; fi++)
                {
                    body.Add(REDISProtocol.CreateREDISBulk(lines[fi]));
                }
            }
        }
    }

    /// <summary>
    /// Rdeis 簇信息
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public class REDISBulk : REDISAction<string>
    {
        /// <summary>
        /// 
        /// </summary>
        public REDISBulk() { }

        /// <summary>
        /// Rdeis 簇信息
        /// </summary>
        /// <param name="sr"></param>
        public REDISBulk(StreamReader sr)
        {
            this.action = (char)sr.Read();
            //兼容所有基础头
            if (!REDIS.BodyHeaders.Contains(""+action)) throw new Exception(sr.BaseStream.Position + "参数头错误" + action);
            //Bulk时读长度
            if(action== REDIS.Bulk) count = int.Parse(sr.ReadLine());
            body = sr.ReadLine();
        }

        /// <summary>
        /// Rdeis 簇信息
        /// </summary>
        /// <param name="br"></param>
        public REDISBulk(BinaryReader br)
        {
            var ch = (char)br.ReadByte();
            this.action =ch;
            //兼容所有基础头
            if (!REDIS.BodyHeaders.Contains(""+action)) throw new Exception(br.BaseStream.Position+ "参数头错误" + action + ((int)action).ToString());
            //Bulk时读长度
            if (action == REDIS.Bulk)
            {
                count = int.Parse(br.ReadLine2());
                body = br.ReadLine2(count);
            }
            else
            {
                body = br.ReadLine2();
            }
        }

        /// <summary>
        /// Rdeis动作
        /// </summary>
        /// <param name="c"></param>
        /// <param name="lines"></param>
        public REDISBulk(char c, string[] lines)
        {
            action = c;
            if (c != REDIS.Bulk) throw new Exception("参数头错误"+c);
            count = int.Parse(lines[0]);
            if (count == -1)
            {
                body = null;
            }
            else
            {
                body = lines[1];
            }
        }
    }

    #endregion

    /// <summary>
    /// 命令项
    /// </summary>
    [Author("Linyee", "2019-01-21")]
    public class REDISAction<T>
    {
        /// <summary>
        /// 动作
        /// </summary>
        public char action { get; set; }

        /// <summary>
        /// 数组长度 或字符串长度
        /// </summary>
        public long count { get; set; }

        /// <summary>
        /// 去掉动作之后的所有内容
        /// </summary>
        public T body { get; set; }

        /// <summary>
        /// 数组
        /// </summary>
        protected List<REDISAction> list = new List<REDISAction>();

        /// <summary>
        /// 
        /// </summary>
        public REDISAction() { }

        /// <summary>
        /// 转字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (action)
            {
                case REDIS.Simple:
                case REDIS.Errors:
                case REDIS.Integer:
                    return action + body.ToString()+"\n";
                case REDIS.Bulk:
                    return action +count+"\n"+ body.ToString() + "\n";
                case REDIS.Arrays:
                    return action + count + "\n" + string.Join("\n", list);
                default:
                    return action + body.ToString() + "\n";
            }
        }

        /// <summary>
        /// 转为非泛型
        /// </summary>
        /// <returns></returns>
        public REDISAction ToREDISCommand()
        {
            return new REDISAction()
            {
                action = action,
                body = body,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public static implicit operator REDISAction(REDISAction<T> obj)
        {
            return obj.ToREDISCommand();
        }
    }
}

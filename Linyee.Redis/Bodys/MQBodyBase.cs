using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WS_Core.Bodys
{
    /// <summary>
    /// 实体内容基类
    /// 用于Mq，自带时间戳、签名
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class MQBodyBase
    {
        /// <summary>
        /// 签名密钥
        /// </summary>
        public static string SignKey;

        /// <summary>
        /// 容器
        /// </summary>
        [JsonIgnore]
        public MQReQuestSponseBase Container { get;internal set; }

        /// <summary>
        /// 13位时间戳
        /// </summary>
        public long timestamp { get; set; } = DateTime.Now.GetTimestamp();

        /// <summary>
        /// 时间
        /// </summary>
        [JsonIgnore]
        public DateTime CreateTime =>timestamp.TimestampToDateTime();

        /// <summary>
        /// 刷新时间戳
        /// </summary>
        /// <returns></returns>
        public MQBodyBase FlushTimestamp()
        {
            this.timestamp = DateTime.Now.GetTimestamp();
            return this;
        }

        /// <summary>
        /// 签名
        /// </summary>
        public string sign { get; set; }

        /// <summary>
        /// 获取签名
        /// 注意：只针对公共读写属性，无缝拼接，大写化
        /// </summary>
        /// <returns></returns>
        public virtual string GetSign(string signkey = null)
        {
            SortedDictionary<string, string> sdict = new SortedDictionary<string, string>();
            foreach(var p in this.GetType().GetProperties())
            {
                if (p.Name == "sign") continue;//跳过签名字符
                if (p.GetSetMethod() == null || p.GetSetMethod().IsPrivate) continue;//跳过非完全公开属性
                var val = p.GetValue(this);
                if (val == null || val.ToString() == "") continue;//跳过空值
                sdict.Add(p.Name, val.ToString());
            }

            var signstr =string.Join("", sdict.Select(p=>p.Value)) + (signkey ?? SignKey);
            var signrst = signstr.ToMd5String();
            LogService.SignRuntime("Body内容签名源串：", signstr);
            LogService.SignRuntime("Body内容签名结果：", signrst);
            return signrst;
        }
    }

    /// <summary>
    /// 请求 实体内容基类
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class QuestBodyBase : MQBodyBase { }

    /// <summary>
    /// 响应 实体内容基类
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class SponseBodyBase : MQBodyBase { }
}

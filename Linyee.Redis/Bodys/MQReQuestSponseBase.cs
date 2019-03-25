using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WS_Core.Consts;

namespace WS_Core.Bodys
{
    #region 请求 响应基础
    /// <summary>
    /// 请求 响应基础
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class MQReQuestSponseBase: MQDataBase
    {
        /// <summary>
        /// 构造
        /// </summary>
        public MQReQuestSponseBase()
        {
            ContentType = MIME.json;
        }


        /// <summary>
        /// 请求id
        /// </summary>
        public string RequestId = Guid.NewGuid().ToString("N");


        #region "内容"
        /// <summary>
        /// 实体内容
        /// </summary>
        [JsonIgnore]
        public object Body { get; set; }


        /// <summary>
        /// 文本内容
        /// </summary>
        public string Content
        {
            get
            {
                if (ContentType.Equals(MIME.json, StringComparison.OrdinalIgnoreCase))
                {
                    return Body.ToJsonString();
                }
                else
                {
                    return BodyStream.ToString();
                }
            }
            set
            {
                BodyStream.Clear();
                BodyStream.Append(value);
                if (ContentType.Equals(MIME.json, StringComparison.OrdinalIgnoreCase))
                {
                    if (value.StartsWith("{") && value.EndsWith("}"))
                    {
                        try
                        {
                            Body = JsonConvert.DeserializeObject(value);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    else
                    {
                        throw new Exception(MIME.json + "数据格式不正确");
                    }
                }
            }
        }
        #endregion

        #region 内容流

        /// <summary>
        /// 追加内容
        /// </summary>
        /// <param name="str"></param>
        public StringBuilder Append(string str)
        {
            return BodyStream.Append(str);
        }

        /// <summary>
        /// 追加内容
        /// </summary>
        /// <param name="str"></param>
        public StringBuilder AppendLine(string str)
        {
            return BodyStream.AppendLine(str);
        }

        /// <summary>
        /// 追加 格式内容
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public StringBuilder AppendFormat(string format, params object[] args)
        {
            return BodyStream.AppendFormat(format, args);
        }

        /// <summary>
        /// 追加 串联内容
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="values"></param>
        public StringBuilder AppendJoin(string separator, params string[] values)
        {
            return BodyStream.Append(string.Join( separator, values));
        }

        /// <summary>
        /// 追加 串联内容
        /// </summary>
        /// <param name="separator"></param>
        /// <param name="values"></param>
        public StringBuilder AppendJoin(string separator, params object[] values)
        {
            return BodyStream.Append(string.Join(separator, values));
        }

        /// <summary>
        /// 内容
        /// </summary>
        internal StringBuilder BodyStream = new StringBuilder();
        #endregion

        #region 头信息
        /// <summary>
        /// 头信息
        /// </summary>
        internal Dictionary<string, string> Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// 文档类别
        /// </summary>
        public string ContentType
        {
            get
            {
                if (Headers.ContainsKey("Content-Type")) return Headers["Content-Type"]; else return null;
            }
            set
            {
                if (Headers.ContainsKey("Content-Type")) Headers["Content-Type"] = value; else Headers.Add("Content-Type", value);
            }
        }
        #endregion
    }


    /// <summary>
    /// 请求 响应基础
    /// </summary>
    [Author("Linyee", "2019-01-28")]
    public class MQReQuestSponseBase<T> : MQReQuestSponseBase
        where T: MQBodyBase
    {

        #region "内容"
        /// <summary>
        /// 实体内容
        /// </summary>
        [JsonIgnore]
        public new T Body { get { return _body; } set { _body = value; _body.Container = this; } }
        private T _body;


        /// <summary>
        /// 文本内容
        /// </summary>
        public new string Content
        {
            get
            {
                if (ContentType.Equals(MIME.json, StringComparison.OrdinalIgnoreCase))
                {
                    return Body.ToJsonString();
                }
                else
                {
                    return BodyStream.ToString();
                }
            }
            set
            {
                BodyStream.Clear();
                BodyStream.Append(value);
                if (ContentType.Equals(MIME.json, StringComparison.OrdinalIgnoreCase))
                {
                    if (value.StartsWith("{") && value.EndsWith("}"))
                    {
                        try
                        {
                            Body = JsonConvert.DeserializeObject<T>(value);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    else
                    {
                        throw new Exception(MIME.json + "数据格式不正确");
                    }
                }
            }
        }
        #endregion

    }
    #endregion


}

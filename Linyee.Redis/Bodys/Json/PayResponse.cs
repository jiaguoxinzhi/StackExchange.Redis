using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WS_Core.Bodys;
using WS_Core.Enums;

namespace WS_Core.Bodys.Json
{
    /// <summary>
    /// 支付响应数据
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class PayResponse : SponseBodyBase
    {

        /// <summary>
        /// 
        /// </summary>
        public PayResponse() { }

        /// <summary>
        /// 用户id
        /// </summary>
        public long id { get; set; }
        /// <summary>
        /// 类别
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 产品
        /// </summary>
        public string product { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal money { get; set; }
        /// <summary>
        /// 当前余额
        /// </summary>
        public string balance { get; set; }
        /// <summary>
        /// 当前余额
        /// </summary>
        [JsonIgnore]
        public decimal Balance { get { return decimal.Parse("0" + balance); } set { balance = value.ToString("0.00"); } }
        /// <summary>
        /// 备注 作为订单号使用
        /// </summary>
        public string mark { get; set; }

        /// <summary>
        /// 返回码 映射到Http状态码
        /// 200 正常 -200错误 状态码 自定义状态码
        /// </summary>
        public StatusCodeEnum code { get { return _code; }  set { _code = value;msg = ConstEnum.GetEnumDescription(value); } }
        private StatusCodeEnum _code;

        /// <summary>
        /// 返回消息
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string payurl { get; set; }

        /// <summary>
        /// 收款账号
        /// </summary>
        public string account { get; set; }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <returns></returns>
        public override string GetSign(string signkey = null)
        {
            var signstr = account+balance + code+ id + mark + money.ToString("#.00") + msg+ payurl+ product + timestamp + type + (signkey ?? SignKey);
            var signrst = signstr.ToMd5String();
            LogService.SignRuntime("App响应签名源串：", signstr);
            LogService.SignRuntime("App响应签名结果：", signrst);
            return signrst;
        }

        /// <summary>
        /// 超时
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static PayResponse Timeout(int timeout)
        {
            return new PayResponse()
            {
                code= StatusCodeEnum.Request_Timeout,
            };
        }
    }
}

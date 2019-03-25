using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WS_Core.Bodys;

namespace WS_Core.Bodys.Json
{
    /// <summary>
    /// 支付请求信息
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class PayRequest: QuestBodyBase
    {

        /// <summary>
        /// 
        /// </summary>
        public PayRequest(long id, string type, decimal money, string mark,string product, long timestamp, string sign)
        {
            this.id = id;
            this.type = type;
            this.Money = money;
            this.mark = mark;
            this.timestamp = timestamp;
            this.sign = sign;
        }

        /// <summary>
        /// 
        /// </summary>
        public PayRequest()
        {
        }

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
        public string money {
            get { return Money.ToString("#.00"); }
            set
            {
                Money=decimal.Parse(money);
            }
        }
        /// <summary>
        /// 价格
        /// </summary>
        [JsonIgnore]
        public decimal Money { get; set; }
        /// <summary>
        /// 备注 作为订单号使用
        /// </summary>
        public string mark { get; set; }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <returns></returns>
        public override string GetSign(string signkey = null) {
            var signstr = id + mark + money + product + timestamp + type + (signkey ?? SignKey);
            var signrst = signstr.ToMd5String();
            LogService.SignRuntime("向App请求参数签名源串：", signstr);
            LogService.SignRuntime("向App请求参数签名结果：", signrst);
            return signrst;
        }
    }
}

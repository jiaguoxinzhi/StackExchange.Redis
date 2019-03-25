using System;
using System.Collections.Generic;
using System.Text;

namespace WS_Core.Bodys.Json
{
    /// <summary>
    /// 会员登录信息
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class MQMemberLogonInfo: SponseBodyBase
    {
        /// <summary>
        /// 登录名
        /// </summary>
        public string MemberEmail { get; set; }
        /// <summary>
        /// 标识 
        /// </summary>
        public long MemberId { get; set; }
        /// <summary>
        /// 真实手机号
        /// </summary>
        public string MemberPhoneNumber { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string MemberRicardName { get; set; }

        /// <summary>
        /// 自动签名
        /// </summary>
        /// <returns></returns>
        public MQMemberLogonInfo SetSign()
        {
            this.sign = GetSign();
            return this;
        }
    }
}

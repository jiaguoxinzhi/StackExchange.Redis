using System;
using System.Collections.Generic;
using System.Text;

namespace WS_Core.WSBLL
{
    /// <summary>
    /// 登录信息类
    /// </summary>
    [Author("Linyee","2019-03-13")]
    public interface ILoginInfo
    {
        /// <summary>
        /// 
        /// </summary>
        string LoginEmail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string LoginPassword { get; set; }
        /// <summary>
        /// 
        /// </summary>
        DateTime LoginLastLoginTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        long LoginId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int LoginStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string LoginPhoneNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string LoginRicardName { get; set; }
    }

    /// <summary>
    /// 登录信息
    /// </summary>
    [Author("Linyee", "2019-03-13")]
    public class LoginInfo : ILoginInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string LoginEmail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LoginPassword { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime LoginLastLoginTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long LoginId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int LoginStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LoginPhoneNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LoginRicardName { get; set; }
    }
}

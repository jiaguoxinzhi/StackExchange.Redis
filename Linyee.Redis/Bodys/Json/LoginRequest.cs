using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WS_Core;
using WS_Core.Bodys;

namespace WS_Core.Bodys.Json
{
    /// <summary>
    /// 
    /// </summary>
    public static class LoginRequest_ex
    {
    }


    /// <summary>
    /// 登录类
    /// </summary>
    [Author("Linyee", "2019-01-29")]
    public class LoginRequest : QuestBodyBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginAccount"></param>
        /// <param name="loginPassword"></param>
        /// <param name="loginType"></param>
        /// <param name="sign"></param>
        public LoginRequest(string loginAccount, string loginPassword, int loginType, string sign):this()
        {
            this.loginAccount = loginAccount;
            this.loginPassword = loginPassword;
            this.loginType = loginType;
            this.sign = sign;
        }

        /// <summary>
        /// 
        /// </summary>
        public LoginRequest()
        {
        }

        /// <summary>
        /// 登录账号
        /// </summary>
        public string loginAccount { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string loginPassword { get; set; }

        /// <summary>
        /// 登录密码
        /// 解析时使用
        /// </summary>
        [JsonIgnore]
        public string DePassword => CryptTools.DesDecryptFixKey(loginPassword).ToMd5Password();

        /// <summary>
        /// 设置签名
        /// </summary>
        /// <returns></returns>
        public LoginRequest SetSign()
        {
            this.sign = GetSign();
            return this;
        }

        /// <summary>
        /// 刷新时间戳
        /// </summary>
        /// <returns></returns>
        public new LoginRequest FlushTimestamp()
        {
            this.timestamp= DateTime.Now.GetTimestamp();
            return this;
        }

        /// <summary>
        /// 类别
        /// </summary>
        public int loginType { get; set; }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <returns></returns>
        public override string GetSign(string signkey = null)
        {
            var signstr = loginAccount + loginPassword + loginType + timestamp + (signkey ?? SignKey);
            var signrst = signstr.ToMd5String();
            LogService.SignRuntime("App登录签名源串：", signstr);
            LogService.SignRuntime("App登录签名结果：", signrst);
            return signrst;
        }

        /// <summary>
        /// 克隆主要参数
        /// timestamp sign不复制
        /// </summary>
        /// <returns></returns>
        public LoginRequest Clone()
        {
            return new LoginRequest()
            {
                Container = this.Container,
                loginAccount = this.loginAccount,
                loginPassword = this.loginPassword,
                loginType = this.loginType,
                
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WS_Core.Bodys.Json;

namespace WS_Core.BLL
{
    /// <summary>
    /// 会员类
    /// </summary>
    public interface IMemberBll
    {
        /// <summary>
        /// 
        /// </summary>
        Task<ExecuteResult<MQMemberLogonInfo>> CheckLoginAsync(string loginAccount, string loginPassword,int loginType);
        /// <summary>
        /// 
        /// </summary>
        Task<ExecuteResult<MQMemberLogonInfo>> CheckLoginAsync(string loginAccount, string loginPassword,int loginType, Func<string, string, int,Task<ExecuteResult<MQMemberLogonInfo>>> func);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Core.Enums
{
    /// <summary>
    /// 商户状态描述
    /// %10==1 或&lt;0 不允许登录
    /// </summary>
    public enum MemberStatusEnum
    {
        /// <summary>
        /// 
        /// </summary>
        已删除=0,
        /// <summary>
        /// 
        /// </summary>
        正常 = 1,
        /// <summary>
        /// 
        /// </summary>
        已锁定 = 2,

        /// <summary>
        /// 正常买家，可以购买，但不能出售
        /// </summary>
        正常买家 = 11,

        /// <summary>
        /// 商户套餐异常，可以登录，但不能交易
        /// </summary>
        商户套餐异常 = 21,
        /// <summary>
        /// 手续费不足，可以登录，但不能交易
        /// </summary>
        手续费不足 = 31,
        /// <summary>
        /// 余额不足，可以登录，但不能交易
        /// </summary>
        余额不足 = 41,
        /// <summary>
        /// 余额不足，可以登录，但不能交易
        /// </summary>
        借记金额已超出 = 51,
        /// <summary>
        /// 费率异常，可以登录，但不能交易
        /// </summary>
        费率异常 = 61,
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Core.Enums
{
    /// <summary>
    /// 定单类型
    /// </summary>
    public enum OrderTypeCode
    {
        /// <summary>
        /// 新购服务
        /// </summary>
        [Description("新购服务")]
        新购服务 = 1,
        /// <summary>
        /// 手动续费服务
        /// </summary>
        [Description("手动续费服务")]
        手动续费服务 = 2,
        /// <summary>
        /// 自动继续服务
        /// </summary>
        [Description("自动继续服务")]
        自动继续服务 = 1002,
        /// <summary>
        /// 升级服务
        /// </summary>
        [Description("升级服务")]
        升级服务 = 3,
        /// <summary>
        /// 降级服务
        /// </summary>
        [Description("降级服务")]
        降级服务 = 4,
        /// <summary>
        /// 冲值余额
        /// </summary>
        [Description("冲值余额")]
        冲值余额 = 5,
        /// <summary>
        /// 手费续模式
        /// </summary>
        [Description("手费续")]
        手费续 = 6,
        /// <summary>
        /// 交易额模式
        /// </summary>
        [Description("交易额")]
        交易额 = 7,
    }
}

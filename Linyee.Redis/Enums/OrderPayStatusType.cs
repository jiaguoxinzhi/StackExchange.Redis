using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace WS_Core.Enums
{
    /// <summary>
    /// 商户状态
    /// 渠道状态
    /// </summary>
    public enum MemberStatus
    {
        /// <summary>
        /// 删除的
        /// </summary>
        [Description("已删除")]
        Deleted = 0,
        /// <summary>
        /// 正常的
        /// </summary>
        [Description("正常")]
        Normal = 1,
        /// <summary>
        /// 关闭的
        /// </summary>
        [Description("已关闭")]
        Closed = 2,
    }


    /// <summary>
    /// 支付类型
    /// </summary>
    public enum OrderPayStatusEnum : int
    {
        /// <summary>
        /// 
        /// </summary>
        未支付 = 0,
        /// <summary>
        /// 
        /// </summary>
        已支付 = 1,
        /// <summary>
        /// 
        /// </summary>
        已入账 = 2,
        /// <summary>
        /// 
        /// </summary>
        已收款 = 3,
        /// <summary>
        /// 
        /// </summary>
        已入款 = 4,
        /// <summary>
        /// 
        /// </summary>
        代入款 = 5,
        /// <summary>
        /// 
        /// </summary>
        代收款 = 6,
    }

    /// <summary>
    /// App支付类型
    /// </summary>
    public enum AppOrderPayStatusEnum : int
    {
        /// <summary>
        /// 
        /// </summary>
        待支付 = 0,
        /// <summary>
        /// 
        /// </summary>
        未知 = -1,
        /// <summary>
        /// 
        /// </summary>
        已支付 = 1,
        /// <summary>
        /// 
        /// </summary>
        已关闭 = 2,
        /// <summary>
        /// 
        /// </summary>
        已转出 = 101,
    }

    /// <summary>
    /// 下单api方式
    /// </summary>
    public enum OrderApiTypeEnum : int
    {
        /// <summary>
        /// 
        /// </summary>
        未知 = 0,
        /// <summary>
        /// 
        /// </summary>
        PayJson = 1,
        /// <summary>
        /// 
        /// </summary>
        PayHtml = 2,
        /// <summary>
        /// 
        /// </summary>
        PayChannelJson = 3,
        /// <summary>
        /// 
        /// </summary>
        PayChannelHtml = 4,
        /// <summary>
        /// 
        /// </summary>
        PayChannelRevJson = 5,
        /// <summary>
        /// 
        /// </summary>
        PayChannelRevHtml = 6,
        /// <summary>
        /// 
        /// </summary>
        PayBuyerJson = 7,
        /// <summary>
        /// 
        /// </summary>
        PayBuyerHtml = 8,
        /// <summary>
        /// 
        /// </summary>
        PPayChannelTradJson = 9,
        /// <summary>
        /// 
        /// </summary>
        PayChannelTradHtml = 10,
        /// <summary>
        /// 
        /// </summary>
        PPayChannelRedbagJson = 11,
        /// <summary>
        /// 
        /// </summary>
        PayChannelRedbagHtml = 12,
    }

    /// <summary>
    /// 支付方式
    /// </summary>
    public enum MemberOrderIsTypeEnum : int
    {
        /// <summary>
        /// 
        /// </summary>
        支付宝 = 1,
        /// <summary>
        /// 
        /// </summary>
        微信 = 2,
        /// <summary>
        /// 
        /// </summary>
        银联 = 3,
        /// <summary>
        /// 
        /// </summary>
        QQ = 5,
        /// <summary>
        /// 
        /// </summary>
        京东 = 6,
        /// <summary>
        /// 
        /// </summary>
        未知方式 = 0
    }
}

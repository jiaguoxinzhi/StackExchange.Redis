using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Core.Enums
{
    /// <summary>
    /// 用户类型
    /// </summary>
    public enum UserTypeEnum
    {
        /// <summary>
        /// 
        /// </summary>
         未知=0,
         /// <summary>
         /// 
         /// </summary>
         User=1,
         /// <summary>
         /// 
         /// </summary>
         Channel=2,
         /// <summary>
         /// 
         /// </summary>
         Member=3,
         /// <summary>
         /// 
         /// </summary>
         Clerk=4,
         /// <summary>
         /// 
         /// </summary>
        ClerkUser=4001,
        /// <summary>
        /// 
        /// </summary>
        ClerkChannel = 4002,
        /// <summary>
        /// 
        /// </summary>
        ClerkMember = 4003,
    }
}

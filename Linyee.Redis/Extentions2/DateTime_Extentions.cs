using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// 时间日期
    /// </summary>
    public static class DateTime_Extentions
    {
        /// <summary>
        /// 13位时间戳
        /// 从1970-1-1开始毫秒数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetTimestamp(this DateTime dt)
        {
            return (long)(dt - new DateTime(1970, 1, 1).ToLocalTime()).TotalMilliseconds;
        }

        /// <summary>
        /// 从13位时间戳获取时间
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>

        public static DateTime TimestampToDateTime(this long ts)
        {
            DateTime dt=DateTime.MinValue;
            var tsstr = ts.ToString();
            if (tsstr.Length == 13)
            {
                dt = new DateTime(1970, 1, 1).AddMilliseconds(ts);
            }
            else if (tsstr.Length == 10)
            {
                dt = new DateTime(1970, 1, 1).AddSeconds(ts);
            }
            else
            {
                throw new Exception("不是有效时间戳");
            }
        return dt.ToLocalTime();
        }

        /// <summary>
        /// 从13位时间戳获取时间
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="ts"></param>
        /// <returns></returns>

        public static DateTime GetFromTimestamp(this DateTime dt,long ts)
        {
            var tsstr = ts.ToString();

            if (tsstr.Length == 13)
            {
                dt = new DateTime(1970, 1, 1).AddMilliseconds(ts);
            }
            else if (tsstr.Length == 10)
            {
                dt = new DateTime(1970, 1, 1).AddSeconds(ts);
            }
            else
            {
                    throw new Exception("不是有效时间戳");
            }
            return dt;
        }
    }
}

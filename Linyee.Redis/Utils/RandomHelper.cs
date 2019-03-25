using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WS_Core
{
    /// <summary>
    /// 尽量不重复的随机数
    /// Linyee 2018-10-03
    /// </summary>
    public class RandomHelper
    {
        private static int lstint = 0;
        private static long lstlng = 0;

        /// <summary>
        /// 随机整数
        /// </summary>
        /// <returns></returns>
        public static int GetRanInt()
        {
            long tick = DateTime.Now.Ticks;
            Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
            var cint= ran.Next();
            while(cint== lstint)
            {
                cint = ran.Next();
            }
            lstint = cint;
            return cint;
        }
        /// <summary>
        /// 随机长整数
        /// </summary>
        /// <returns></returns>
        public static long GetRanLng()
        {
            long tick = DateTime.Now.Ticks;
            Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
            var i1= ran.Next();
            var i2 = ran.Next();
            var clng= (i2 << 32) | i1;
            while (clng == lstlng)
            {
                clng = ran.Next();
            }
            lstlng = clng;
            return clng;
        }

        /// <summary>
        /// 随机id
        /// </summary>
        /// <returns></returns>
        public static Guid GetNewId()
        {
            return Guid.NewGuid();
        }
    }
}

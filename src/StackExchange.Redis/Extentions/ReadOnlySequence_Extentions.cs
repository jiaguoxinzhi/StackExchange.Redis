using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
    /// <summary>
    /// 
    /// </summary>
    public static class ReadOnlySequence_Extentions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="linelen"></param>
        /// <returns></returns>
        public static StringBuilder ToHexString(this ReadOnlySequence<byte> sequence,int linelen = 32)
        {
            StringBuilder sbd = new StringBuilder();
            var buf = sequence.ToArray();
            for (var fi = 0; fi < buf.Length; fi++)
            {
                sbd.Append(buf[fi].ToString("X2"))
                    .Append("\\")
                    .Append((""+(char)buf[fi]).Replace("\r","r").Replace("\n", "n").Replace("\t", "t").Replace("\v", "v").Replace("\x7F", " "))
                    .Append(" ");

                if ((fi + 1) % 32 == 0) sbd.Append("\r\n");
            }
            return sbd;
        }
    }
}

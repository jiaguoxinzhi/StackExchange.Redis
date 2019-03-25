using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// 字符串 扩展
    /// Linyee 2018-06-29
    /// </summary>
    [Author("Linyee", "2019-02-01")]
    public static class String_Extentions
    {

        #region "转换"
        /// <summary>
        /// 转为字节组
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] ToUTF8Bytes(this string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
        /// <summary>
        /// 获取字节组
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] GetUTF8Bytes(this string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        /// <summary>
        /// 转安全显示字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="l"></param>
        /// <param name="c"></param>
        /// <param name="r"></param>
        /// <param name="cleng"></param>
        /// <returns></returns>
        public static string ToSafeShow(this String str, int l, char c, int r, int cleng = 3)
        {
            var lstr = str.Left(l);
            var rstr = str.Right(r);
            return lstr + new string(c, cleng) + rstr;
        }

        /// <summary>
        /// 转十进制数
        /// Linyee 2018-05-09
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static decimal ToDecimal(this string str)
        {
            decimal dec = 0;
            Decimal.TryParse(str, out dec);
            return dec;
        }

        /// <summary>
        /// 转为时间间隔
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static TimeSpan ToTimeSpan(this string str)
        {
            TimeSpan ts = new TimeSpan();
            TimeSpan.TryParse(str, out ts);
            return ts;
        }
        #endregion

        #region "判断"
        /// <summary>
        /// 判断 指定的字符串是否 null 或 System.String.Empty 字符串。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return String.IsNullOrEmpty(str);
        }
        /// <summary>
        /// 判断 指定的字符串是否 有效值。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasValue(this string str)
        {
            return str?.Trim().Length>0;
        }
        #endregion

        #region "截断"
        /// <summary>
        /// 右截
        /// Linyee 2018-05-07
        /// </summary>
        /// <param name="str"></param>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public static string Right(this string str, int lenght)
        {
            if (str == null) return string.Empty;
            if (str.Length <= lenght) return str;
            return str.Substring(str.Length - lenght);
        }

        /// <summary>
        /// 左截
        /// Linyee 2018-05-07
        /// </summary>
        /// <param name="str"></param>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public static string Left(this string str, int lenght)
        {
            if (str == null) return string.Empty;
            if (str.Length <= lenght) return str;
            return str.Substring(0, lenght);
        }
        #endregion

        #region "Md5"
        /// <summary>
        /// 转Md5 大写化
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToMd5String(this String str)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(str));

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("X2"));
            }
            string val = sBuilder.ToString();
            return val;
        }

        //Md5密码附加串
        private static string sp_PassAddStr = "percode";

        /// <summary>
        /// 转Md5密码串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToMd5Password(this string str)
        {
            return (str + sp_PassAddStr).ToMd5String();
        }
        #endregion
    }
}

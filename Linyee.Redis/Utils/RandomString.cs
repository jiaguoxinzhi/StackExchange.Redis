using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WS_Core.Utils
{
    /// <summary>
    /// 随机字符串工具
    /// </summary>
    public static class RandomString
    {
        private const string sCharLow = "abcdefghijklmnopqrstuvwxyz";
        private const string sCharUpp = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string sNumber = "0123456789";

        /// <summary>
        /// 随机字符串
        /// </summary>
        /// <param name="strLen"></param>
        /// <param name="StrOf"></param>
        /// <returns></returns>
        public static string BuildRndString(int strLen, string StrOf= sCharLow+ sCharUpp+ sNumber)
        {
            if (StrOf == null) StrOf = sCharLow + sCharUpp + sNumber;

            System.Random RandomObj = new System.Random(RandomNumber.GetNewSeed());
            string buildRndCodeReturn = null;
            for (int i = 0; i < strLen; i++)
            {
                buildRndCodeReturn += StrOf.Substring(RandomObj.Next(0, StrOf.Length - 1), 1);
            }
            return buildRndCodeReturn;
        }

    }
}

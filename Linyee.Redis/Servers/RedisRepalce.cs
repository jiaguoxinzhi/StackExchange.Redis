using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using WS_Core.dyCompiler;
using WS_Core.Enums;

namespace WS_Server.Servers
{

    /// <summary>
    /// 替换工具
    /// </summary>
    internal class RedisRepalce
    {
        private LinyeeWebSocketConnectionBase client;
        /// <summary>
        /// 新值
        /// </summary>
        public string valText { get; private set; }

        internal readonly ExecuteResult<string> result = new ExecuteResult<string>();

        /// <summary>
        /// 替换工具
        /// </summary>
        /// <param name="client"></param>
        /// <param name="text"></param>
        public RedisRepalce(LinyeeWebSocketConnectionBase client, string text)
        {
            result.SetOk();
            this.client = client;
            valText = text;

            valText = ExpressionEval.spaceBodyRegex.Replace(valText, "");//去掉空字串
            valText = ExpressionEval.hexBodyRegex.Replace(valText, ParseBigInteger);//计算十六进制数
            valText = ExpressionEval.dblBodyOnlyRegex.Replace(valText, ParseBigInteger);//小数
            valText = ExpressionEval.lngBodyRegex.Replace(valText, ParseBigInteger);//整数
            valText = ExpressionEval.lngFactorialOnlyRegex.Replace(valText, PaserBigFactorial);//阶乘

            //队列值
            var queregex = (new Regex("[Qq][Uu][Ee]\\-\\>[\\w]+", RegexOptions.Compiled));
            while (queregex.IsMatch(valText) && result.Code == StatusCodeEnum.OK)
            {
                valText = queregex.Replace(valText, GetQueVal);
            }

            //缓存值
            var regex = (new Regex("[\\w\\.]+\\(?", RegexOptions.Compiled));
            while (regex.IsMatch(valText) && result.Code == StatusCodeEnum.OK)
            {
                valText = regex.Replace(valText, GetKeyVal);
            }

            //Console.WriteLine("替换后："+ valText);
            if (result.Code == StatusCodeEnum.Not_Found) result.SetOk();
        }

        private string GetQueVal(Match match)
        {
            var mkey = match.Value;
            var key = mkey.Substring(5);
            //从队列中获取值
            var res = client.RedisGetQue(key);
            //Console.WriteLine("队列值" + res);
            return res;
        }

        /// <summary>
        /// 解析阶乘
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private string PaserFactorial(Match match)
        {
            var res = ExpressionEval.PaserFactorial(match.Value);
            //Console.WriteLine("解析阶乘" + res);
            return res;
        }


        /// <summary>
        /// 解析数值
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private string PaserValue(Match match)
        {
            var res = ExpressionEval.ParseValue(match.Value);
            //Console.WriteLine("解析数值"+ res);
            return res;
        }

        /// <summary>
        /// 解析数值
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private string ParseBigInteger(Match match)
        {
            var res = ExpressionEval.ParseBigInteger(match.Value);
            //Console.WriteLine("解析数值"+ res);
            return res;
        }

        /// <summary>
        /// 解析阶乘
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private string PaserBigFactorial(Match match)
        {
            var res = ExpressionEval.PaserBigFactorial(match.Value);
            //Console.WriteLine("解析阶乘" + res);
            return res;
        }

        /// <summary>
        /// 获取替换的新值
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private string GetKeyVal(Match match)
        {

            var key = match.Value;
            //Console.WriteLine("单词或数值或函数"+key);

            //函数名 字母开头(结束 跳过
            if ((new Regex("^[a-zA-Z]{1}[\\w\\.]*[\\(]{1}$", RegexOptions.Compiled)).IsMatch(key))
            {
                //Console.WriteLine("==函数名" + key);
                result.Code = StatusCodeEnum.Not_Found;
                return key;
            }

            //非字母开头 跳过
            if (!(new Regex("^[a-zA-Z]{1}[\\w\\.]*$", RegexOptions.Compiled)).IsMatch(key))
            {
                //Console.WriteLine("==非字母开头" + key);
                result.Code = StatusCodeEnum.Not_Found;
                return key;
            }

            //从缓存中获取值
            var res = client.RedisGet(key);
            if (res != null)
            {
                //数值串
                if ((new Regex("^[\\+\\-]?\\d+(\\.?\\d+)?$", RegexOptions.Compiled)).IsMatch(res.Value))
                {
                    if (result.Code == StatusCodeEnum.Not_Found) result.SetOk();
                    return res.Value;
                }
                //表达式
                else if ((new Regex("^[\\+\\-]?\\w+([\\+\\-\\*\\/]{1}\\w+)*$", RegexOptions.Compiled)).IsMatch(res.Value))
                {
                    if (result.Code == StatusCodeEnum.Not_Found) result.SetOk();
                    return "(" + res.Value + ")";
                }
                else
                {
                    result.IsOk = false;
                    result.Msg += (key + "不是一个有效的数值\t");
                }
            }
            else
            {
                result.IsOk = false;
                result.Msg += (key + "键值无效\t");
            }
            return key;
        }
    }
}

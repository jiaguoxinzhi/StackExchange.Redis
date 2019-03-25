using System;
using System.Collections.Generic;
using System.Text;

namespace WS_Core.Consts
{
    /// <summary>
    /// redis定义
    /// </summary>
    public class REDIS
    {
        /// <summary>
        /// 所有头信息
        /// </summary>
        public const string Headers = "+-:$*";
        /// <summary>
        /// 基础头信息
        /// </summary>
        public const string BaseHeaders = "+-:";
        /// <summary>
        /// 基本头信息
        /// </summary>
        public const string BodyHeaders = "+-:$";
        /// <summary>
        ///  简单字符串 Simple Strings
        /// </summary>
        public const char Simple = '+';
        /// <summary>
        ///  错误 Errors
        /// </summary>
        public const char Errors = '-';
        /// <summary>
        ///  整数型 Integer
        /// </summary>
        public const char Integer = ':';
        /// <summary>
        ///  大字符串类型 Bulk Strings
        /// </summary>
        public const char Bulk = '$';
        /// <summary>
        ///  数组类型 Arrays
        /// </summary>
        public const char Arrays = '*';
    }
}

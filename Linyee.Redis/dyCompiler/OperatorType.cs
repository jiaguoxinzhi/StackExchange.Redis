using System;
using System.Collections.Generic;
using System.Text;

namespace WS_Core.dyCompiler
{
    /// <summary>
    /// 运算符
    /// </summary>
    [Author("Linyee", "2019-02-03")]
    public enum OperatorType
    {
        /// <summary>
        /// 
        /// </summary>
        Empty = 0,

        #region "单目"
        /// <summary>
        /// 
        /// </summary>
        Increment = 100,//自增
        /// <summary>
        /// 
        /// </summary>
        Decrement,//自减
        /// <summary>
        /// 阶乘
        /// </summary>
        Factorial,
        /// <summary>
        /// 双阶乘 隔1数
        /// </summary>
        Factorial2,
        #endregion
        #region "双目"
        /// <summary>
        /// 
        /// </summary>
        Add = 200,
        /// <summary>
        /// 
        /// </summary>
        Sub,
        /// <summary>
        /// 
        /// </summary>
        Mul,
        /// <summary>
        /// 
        /// </summary>
        Div,
        /// <summary>
        /// 
        /// </summary>
        Mod,

        /// <summary>
        /// 
        /// </summary>
        Min,
        /// <summary>
        /// 
        /// </summary>
        Max,
        #endregion
        #region "定界"
        /// <summary>
        /// 括号
        /// </summary>
        Bracket = 500,
        /// <summary>
        /// 左括号
        /// </summary>
        BracketLeft,
        /// <summary>
        /// 右括号
        /// </summary>
        BracketRight,
        /// <summary>
        /// 函数左括
        /// </summary>
        FunctionLeft,
        #endregion
    }

    /// <summary>
    /// 值类型
    /// </summary>
    public enum EvalObjectType
    {
        /// <summary>
        /// 
        /// </summary>
        Empty = 0,
        /// <summary>
        /// 
        /// </summary>
        Operator,
        /// <summary>
        /// 
        /// </summary>
        Error,
        /// <summary>
        /// 
        /// </summary>
        Function,

        /// <summary>
        /// 
        /// </summary>
        Int64 = 100,
        /// <summary>
        /// 
        /// </summary>
        UInt64,
        /// <summary>
        /// 
        /// </summary>
        Decimal,
        /// <summary>
        /// 
        /// </summary>
        Double,
        /// <summary>
        /// 
        /// </summary>
        BigInteger,
        /// <summary>
        /// 
        /// </summary>
        BigDecimal,
    }

    /// <summary>
    /// 函数类型
    /// </summary>
    public enum FunctionType
    {
        /// <summary>
        /// 
        /// </summary>
        Empty = 0,
        /// <summary>
        /// 
        /// </summary>
        Sum,
        /// <summary>
        /// 
        /// </summary>
        Avg,
        /// <summary>
        /// 
        /// </summary>
        Min,
        /// <summary>
        /// 
        /// </summary>
        Max,
        /// <summary>
        /// 
        /// </summary>
        Pow,
        /// <summary>
        /// 
        /// </summary>
        Sqr,
        /// <summary>
        /// 
        /// </summary>
        Sqrt,
        /// <summary>
        /// 
        /// </summary>
        Sin,
        /// <summary>
        /// 
        /// </summary>
        Cos,
        /// <summary>
        /// 
        /// </summary>
        ASin,
        /// <summary>
        /// 
        /// </summary>
        ACos,
        /// <summary>
        /// 
        /// </summary>
        PI,
        /// <summary>
        /// 
        /// </summary>
        PI100,
        /// <summary>
        /// 
        /// </summary>
        PI10,
        /// <summary>
        /// 
        /// </summary>
        PI1000,
        /// <summary>
        /// 
        /// </summary>
        PI10000,
        /// <summary>
        /// 
        /// </summary>
        PI100000,
        /// <summary>
        /// 
        /// </summary>
        SinDec,
        /// <summary>
        /// 
        /// </summary>
        CosDec,
        /// <summary>
        /// 
        /// </summary>
        TanDec,
        /// <summary>
        /// 
        /// </summary>
        Tan,
        /// <summary>
        /// 
        /// </summary>
        ATan,
    }
}

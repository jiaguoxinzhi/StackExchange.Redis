using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using WS_Server.dyCompiler;

namespace WS_Core.dyCompiler
{
    /// <summary>
    /// 表达式计算工具
    /// 没同步过来，，重新复原下
    /// </summary>
    [Author("Linyee", "2019-02-03")]
    public static class ExpressionEval
    {

        #region "正则表达式"

        #region "通用"
        /// <summary>
        /// 开始
        /// </summary>
        private static string regHeaderString = "^";
        /// <summary>
        /// 正负号
        /// </summary>
        private static string regPMString = "([\\+\\-]?)";
        /// <summary>
        /// 结束
        /// </summary>
        private static string regEnderString = "$";

        ///// <summary>
        ///// 分隔符 空格或逗号
        ///// </summary>
        //internal readonly static Regex spaceBodyRegex = new Regex("[\x20\\,]+",RegexOptions.Compiled);

        /// <summary>
        /// 分隔符 空格
        /// </summary>
        internal readonly static Regex spaceBodyRegex = new Regex("[\x20]+", RegexOptions.Compiled);

        #endregion

        #region "分数"
        /// <summary>
        /// 分数主体
        /// </summary>
        private static string fenBodyString = "\\((\\d+)[\\+又]{1}(\\d+)\\/(\\d+)\\)";

        /// <summary>
        /// 分数主体
        /// </summary>
        internal readonly static Regex fenBodyRegex = new Regex(fenBodyString, RegexOptions.Compiled);

        /// <summary>
        /// 分数行
        /// </summary>
        internal readonly static Regex fenRegex = new Regex(regHeaderString + regPMString + fenBodyString + regEnderString, RegexOptions.Compiled);
        #endregion

        #region "十六进制 整数"

        /// <summary>
        /// hex
        /// </summary>
        private static string hexBodyString ="0x[0-9A-Fa-f]+(L|UL|l|ul)?";

        /// <summary>
        /// 十六进制 整数 不含正负符号
        /// </summary>
        internal readonly static Regex hexBodyRegex = new Regex(hexBodyString, RegexOptions.Compiled);

        /// <summary>
        /// 十六进制 整数 含正负符号
        /// </summary>
        internal readonly static Regex hexRegex = new Regex(regHeaderString + regPMString + hexBodyString+ regEnderString, RegexOptions.Compiled);
        #endregion

        #region "十进制 小数"

        /// <summary>
        /// 小数
        /// </summary>
        private static string dblBodyString = "\\d+(\\.\\d+)?[DdFf]?";
        /// <summary>
        /// 只允许小数
        /// </summary>
        private static string dblBodyOnlyString = "\\d+((\\.\\d+[DdFf]?)|[DdFf])";

        /// <summary>
        /// 十进制 小数 不含正负符号
        /// </summary>
        internal static Regex dblBodyRegex = new Regex(dblBodyString, RegexOptions.Compiled);

        /// <summary>
        /// 十进制 只允许小数 不含正负符号
        /// </summary>
        internal static Regex dblBodyOnlyRegex = new Regex(dblBodyOnlyString, RegexOptions.Compiled);

        /// <summary>
        /// 十进制 小数 含正负符号
        /// </summary>
        internal static Regex dblRegex = new Regex(regHeaderString + regPMString + dblBodyString + regEnderString, RegexOptions.Compiled);

        /// <summary>
        /// 十进制 只允许小数 含正负符号
        /// </summary>
        internal static Regex dblOnlyRegex = new Regex(regHeaderString + regPMString + dblBodyOnlyString + regEnderString, RegexOptions.Compiled);

        #endregion

        #region "十进制 整数"
        /// <summary>
        /// 数字
        /// </summary>
        private static string lngBodyOnlyString = "(\\d+)";
        /// <summary>
        /// 整数后缀 l ul 
        /// </summary>
        private static string lngBodyEndsString = "(([Uu]?[Ll])?)";
        /// <summary>
        /// 整数
        /// </summary>
        private static string lngBodyString = lngBodyOnlyString+ lngBodyEndsString;
        /// <summary>
        /// 十进制 整数 不含正负符号
        /// </summary>
        internal readonly static Regex lngBodyRegex = new Regex(lngBodyString, RegexOptions.Compiled);

        /// <summary>
        /// 十进制 整数 不含正负符号 不含后缀
        /// </summary>
        internal readonly static Regex lngBodyOnlyRegex = new Regex(lngBodyOnlyString, RegexOptions.Compiled);

        /// <summary>
        /// 十进制 整数 含正负符号
        /// </summary>
        internal static Regex lngRegex=new Regex(regHeaderString + regPMString + lngBodyString + regEnderString, RegexOptions.Compiled);

        #endregion

        #region "十进制 阶乘"
        /// <summary>
        /// 阶乘后缀 !或!!
        /// </summary>
        private static string lngFactorialEndsString = "(\\!{1,2})";
        /// <summary>
        /// 阶乘后缀 !或!!
        /// </summary>
        private static string lngFactorialBodyString = "(\\(" + regPMString + lngBodyOnlyString + "\\)|("+ lngBodyOnlyString + "))";
        /// <summary>
        /// 阶乘 (-5)!
        /// </summary>
        private static string lngFactorialString = lngFactorialBodyString + lngFactorialEndsString;
        /// <summary>
        /// 十进制 阶乘 不含正负符号
        /// </summary>
        internal readonly static Regex lngFactorialOnlyRegex = new Regex(lngFactorialString, RegexOptions.Compiled);
        /// <summary>
        /// 十进制 阶乘 只含主体
        /// </summary>
        internal readonly static Regex lngFactorialBodyOnlyRegex = new Regex(lngFactorialBodyString, RegexOptions.Compiled);

        /// <summary>
        /// 十进制 阶乘 含正负符号
        /// </summary>
        internal static Regex lngFactorialRegex = new Regex(regHeaderString + regPMString + lngFactorialString + regEnderString, RegexOptions.Compiled);

        #endregion



        /// <summary>
        /// 阶乘解析
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string PaserBigFactorial(this string text)
        {
            //空值返回0
            if (string.IsNullOrEmpty(text)) return "";

            //一个数值 十进制 阶乘
            if (lngFactorialRegex.IsMatch(text))
            {
                var body = lngFactorialBodyOnlyRegex.Match(text).Value;
                //Console.WriteLine("阶乘数：{0}-->{1}", text,body);
                var jc = 1;
                if (text.EndsWith("!!")) jc = 2;
                var bl= BigInteger.Parse(body.TrimStart('(').TrimEnd(')'));//去掉括号解析出主值
                BigInteger br = 1L;
                if (bl > 0)
                {
                    if (bl > 999) text = "Error("+bl+"!)";
                    for (var fi = bl; fi>0L ; fi-= jc)
                    {
                        br *= fi;
                    }
                }
                else if (bl < 0)
                {
                    if (bl < -999) text = "Error(" + bl + "!)";
                    for (var fi = bl; fi < 0L; fi += jc)
                    {
                        br *= fi;
                    }
                }
                else
                {
                    return "0";
                }
                return br.ToString();
            }

            return text;
        }

        /// <summary>
        /// 阶乘解析
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string PaserFactorial(this string text)
        {
            //空值返回0
            if (string.IsNullOrEmpty(text)) return "";

            //一个数值 十进制 阶乘
            if (lngFactorialRegex.IsMatch(text))
            {
                var body = lngFactorialBodyOnlyRegex.Match(text).Value;
                //Console.WriteLine("阶乘数：{0}-->{1}", text,body);
                var jc = 1;
                if (text.EndsWith("!!")) jc = 2;
                var bl= long.Parse(body.TrimStart('(').TrimEnd(')'));//去掉括号解析出主值
                var br = 1L;
                if (bl > 0)
                {
                    for (var fi = bl; fi>0L ; fi-= jc)
                    {
                        br *= fi;
                    }
                }
                else if (bl < 0)
                {
                    for (var fi = bl; fi < 0L; fi += jc)
                    {
                        br *= fi;
                    }
                }
                else
                {
                    return "0";
                }
                return br.ToString();
            }

            return text;
        }

        /// <summary>
        /// 数值解析 字符串 无后缀 无前缀
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ParseValue(this string text)
        {
            //空值返回0
            if (string.IsNullOrEmpty(text)) return "";

            //一个数值 十六进制 整数
            if (hexRegex.IsMatch(text))
            {
                if (text.EndsWith("UL", StringComparison.OrdinalIgnoreCase)) return  ulong.Parse(text.Substring(2, text.Length - 4), System.Globalization.NumberStyles.HexNumber).ToString();
                else if (text.EndsWith("L", StringComparison.OrdinalIgnoreCase)) return long.Parse(text.Substring(2, text.Length - 3), System.Globalization.NumberStyles.HexNumber).ToString();
                else return  long.Parse(text.Substring(2), System.Globalization.NumberStyles.HexNumber).ToString();
            }

            //一个数值 十进制 阶乘
            if (lngFactorialRegex.IsMatch(text))
            {
                Console.WriteLine("阶乘数："+ text);
                var bl= long.Parse(text.Substring(0,text.Length-1));
                var br = 1;
                for(var fi = 2; fi <= bl; fi++)
                {
                    br *= fi;
                }
                return br.ToString();
            }

            //一个数值 十进制 小数
            if (dblOnlyRegex.IsMatch(text))
            {
                if (text.EndsWith("d", StringComparison.OrdinalIgnoreCase) || text.EndsWith("f", StringComparison.OrdinalIgnoreCase))
                    return double.Parse(text.Substring(0, text.Length - 1)).ToString("0.00");
                else
                    return double.Parse(text).ToString("0.00");
            }
            else
            //一个数值 十进制 整数
            if (lngRegex.IsMatch(text))
            {
                if (text.EndsWith("UL",StringComparison.OrdinalIgnoreCase)) return  ulong.Parse(text.Substring(0, text.Length - 2)).ToString();
                else if (text.EndsWith("L", StringComparison.OrdinalIgnoreCase)) return long.Parse(text.Substring(0, text.Length - 1)).ToString();
                else return  long.Parse(text).ToString();
            }

            return text;
        }

        /// <summary>
        /// 数值解析 字符串 无后缀 无前缀
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ParseBigInteger(this string text)
        {
            //空值返回0
            if (string.IsNullOrEmpty(text)) return "";

            //一个数值 十六进制 整数
            if (hexRegex.IsMatch(text))
            {
                if (text.EndsWith("UL", StringComparison.OrdinalIgnoreCase)) return BigInteger.Parse(text.Substring(2, text.Length - 4), System.Globalization.NumberStyles.HexNumber).ToString();
                else if (text.EndsWith("L", StringComparison.OrdinalIgnoreCase)) return BigInteger.Parse(text.Substring(2, text.Length - 3), System.Globalization.NumberStyles.HexNumber).ToString();
                else return BigInteger.Parse(text.Substring(2), System.Globalization.NumberStyles.HexNumber).ToString();
            }

            //一个数值 十进制 阶乘
            if (lngFactorialRegex.IsMatch(text))
            {
                //Console.WriteLine("阶乘数：" + text);
                var bl = long.Parse(text.Substring(0, text.Length - 1));
                var br = 1;
                for (var fi = 2; fi <= bl; fi++)
                {
                    br *= fi;
                }
                return br.ToString();
            }

            //一个数值 十进制 小数
            if (dblOnlyRegex.IsMatch(text))
            {
                if (text.EndsWith("d", StringComparison.OrdinalIgnoreCase) || text.EndsWith("f", StringComparison.OrdinalIgnoreCase))
                    return BigDecimal.Parse(text.Substring(0, text.Length - 1)).ToString();
                else
                    return BigDecimal.Parse(text).ToString();
            }
            else
            //一个数值 十进制 整数
            if (lngRegex.IsMatch(text))
            {
                if (text.EndsWith("UL", StringComparison.OrdinalIgnoreCase)) return BigInteger.Parse(text.Substring(0, text.Length - 2)).ToString();
                else if (text.EndsWith("L", StringComparison.OrdinalIgnoreCase)) return BigInteger.Parse(text.Substring(0, text.Length - 1)).ToString();
                else return BigInteger.Parse(text).ToString();
            }

            return text;
        }

        /// <summary>
        /// 数值解析 字符串 无后缀 无前缀
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static object ParseBigVal(this string text)
        {
            //空值返回0
            if (string.IsNullOrEmpty(text)) return 0;

            //一个数值 十六进制 整数
            if (hexRegex.IsMatch(text))
            {
                if (text.EndsWith("UL", StringComparison.OrdinalIgnoreCase)) return BigInteger.Parse(text.Substring(2, text.Length - 4), System.Globalization.NumberStyles.HexNumber);
                else if (text.EndsWith("L", StringComparison.OrdinalIgnoreCase)) return BigInteger.Parse(text.Substring(2, text.Length - 3), System.Globalization.NumberStyles.HexNumber);
                else return BigInteger.Parse(text.Substring(2), System.Globalization.NumberStyles.HexNumber);
            }

            //一个数值 十进制 阶乘
            if (lngFactorialRegex.IsMatch(text))
            {
                //Console.WriteLine("阶乘数：" + text);
                var bl = long.Parse(text.Substring(0, text.Length - 1));
                BigInteger br = 1;
                for (var fi = 2; fi <= bl; fi++)
                {
                    br *= fi;
                }
                return br;
            }

            //一个数值 十进制 小数
            if (dblOnlyRegex.IsMatch(text))
            {
                if (text.EndsWith("d", StringComparison.OrdinalIgnoreCase) || text.EndsWith("f", StringComparison.OrdinalIgnoreCase))
                    return BigDecimal.Parse(text.Substring(0, text.Length - 1));
                else
                    return BigDecimal.Parse(text);
            }
            else
            //一个数值 十进制 整数
            if (lngRegex.IsMatch(text))
            {
                if (text.EndsWith("UL", StringComparison.OrdinalIgnoreCase)) return BigInteger.Parse(text.Substring(0, text.Length - 2));
                else if (text.EndsWith("L", StringComparison.OrdinalIgnoreCase)) return BigInteger.Parse(text.Substring(0, text.Length - 1));
                else return BigInteger.Parse(text);
            }

            return BigInteger.Parse(text);
        }

        /// <summary>
        /// 数值解析
        /// 解析过的值 只可能long ulong double
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static object ParseVal(this string text)
        {
            //Console.WriteLine("解析值"+text);

            //空值返回0
            if (string.IsNullOrEmpty(text)) return 0L;

            //一个数值 十六进制 整数
            if (hexRegex.IsMatch(text))
            {
                if (text.EndsWith("UL")) return  ulong.Parse(text, System.Globalization.NumberStyles.HexNumber);
                else return  long.Parse(text, System.Globalization.NumberStyles.HexNumber);
            }

            if ((text.IndexOf('.') >= 0 || text.EndsWith("d", StringComparison.OrdinalIgnoreCase) || text.EndsWith("f", StringComparison.OrdinalIgnoreCase)) && dblRegex.IsMatch(text))
            {
                //一个数值 十进制 小数
                return double.Parse(text);
            }
            else
            //一个数值 十进制 整数
            if (lngRegex.IsMatch(text))
            {
                if (text.EndsWith("UL",StringComparison.OrdinalIgnoreCase)) return ulong.Parse(text);
                else return long.Parse(text);
            }

            return new EvalObject() { Type = EvalObjectType.Error, Value = "无法解析"+text, };
        }

        #endregion

        #region "表达式计算"
        /// <summary>
        /// 表达式计算
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static EvalObject eval(this string text)
        {
            var valText = text;
            //if (text.IndexOf(":") < 0) valText = ParseValue(valText);
            if (text.IndexOf(":") < 0) valText = ParseBigInteger(valText);

            StringBuilder val = new StringBuilder();
            StringBuilder fun = new StringBuilder();

            Queue<EvalObject> que = new Queue<EvalObject>();
            Stack<EvalObject> st = new Stack<EvalObject>();
            //函数栈
            Stack<EvalFunction> stf = new Stack<EvalFunction>();

            //Console.WriteLine("=进入转换"+ text);

            foreach (var ch in valText)
            {
                switch (ch)
                {
                    case '(':
                        //创建函数
                        if (fun.Length > 0)
                        {
                            var funType =(FunctionType) Enum.Parse(typeof(FunctionType), fun.ToString());
                            var funValue = new EvalFunction() {  Type= EvalObjectType.Function,FunType= funType };
                            stf.Push(funValue);
                            fun.Clear();
                            st.Push(new EvalObject() { Type = EvalObjectType.Function, Value = funValue });
                            st.Push(new EvalObject() { Type = EvalObjectType.Operator, Value = OperatorType.BracketLeft });
                        }
                        //非函数
                        else
                        {
                            st.Push(new EvalObject() { Type = EvalObjectType.Operator, Value = OperatorType.BracketLeft });
                        }
                        break;
                    case ')':
                        //推入到参数
                        pushB(val, que);
                        //计算参数
                        js2(st, que, OperatorType.BracketRight);

                        //如果当前是函数
                        var tval = st.Peek();
                        if (tval.Type== EvalObjectType.Function)
                        {
                            st.Pop();
                            var func = (EvalFunction)tval.Value;
                            stf.Peek().Add(que.Dequeue());

                            //推入函数结果
                            que.Enqueue(stf.Pop().Value);
                        }

                        break;
                    case '+':
                        pushB(val, que);
                        if (que.Count==0)//正号
                        {
                            val.Append(ch);
                        }
                        else
                        if (st.Count == 0 && que.Count > 0)//第一个运算符
                        {
                            st.Push(que.Dequeue());
                            st.Push(new EvalObject() { Type = EvalObjectType.Operator, Value = OperatorType.Add });
                        }
                        else
                        {
                            js2(st, que, OperatorType.Add);
                            st.Push(que.Dequeue());
                            st.Push(new EvalObject() { Type = EvalObjectType.Operator, Value = OperatorType.Add });
                        }
                        break;
                    case '-':
                        pushB(val, que);
                        if (que.Count == 0)
                        {
                            val.Append(ch);
                        }
                        else
                        if (st.Count == 0 && que.Count > 0)
                        {
                            st.Push(que.Dequeue());
                            st.Push(new EvalObject() { Type = EvalObjectType.Operator, Value = OperatorType.Sub });
                        }
                        else
                        {
                            js2(st, que, OperatorType.Sub);
                            st.Push(que.Dequeue());
                            st.Push(new EvalObject() { Type = EvalObjectType.Operator, Value = OperatorType.Sub });
                        }
                        break;
                    case '*':
                        {
                            pushB(val, que);
                            js2(st, que, OperatorType.Mul);
                            st.Push(que.Dequeue());
                        }
                        st.Push(new EvalObject() { Type = EvalObjectType.Operator, Value = OperatorType.Mul });
                        break;
                    case '/':
                        {
                            pushB(val, que);
                            js2(st, que, OperatorType.Div);
                            st.Push(que.Dequeue());
                        }
                        st.Push(new EvalObject() { Type = EvalObjectType.Operator, Value = OperatorType.Div });
                        break;
                    case '%':
                        {
                            pushB(val, que);
                            js2(st, que, OperatorType.Mod);
                            st.Push(que.Dequeue());
                        }
                        st.Push(new EvalObject() { Type = EvalObjectType.Operator, Value = OperatorType.Mod });
                        break;
                    case '!':
                        bool isFactorial1 = false;
                        if (st.Count > 0)
                        {
                            var tmp = st.Peek();
                            if (tmp.Type == EvalObjectType.Operator && (OperatorType)tmp.Value == OperatorType.Factorial)
                            {
                                //双阶乘
                                st.Pop();
                                st.Push(new EvalObject() { Type = EvalObjectType.Operator, Value = OperatorType.Factorial2 });
                            }
                            else
                            {
                                isFactorial1 = true;
                            }
                        }
                        else
                        {
                            isFactorial1 = true;
                        }

                        if(isFactorial1)
                        {
                            st.Push(new EvalObject() { Type = EvalObjectType.Operator, Value = OperatorType.Factorial });
                        }
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        val.Append(ch);
                        break;
                    //case 'd':
                    //    val.Append(ch);
                    //    break;
                    //case 'D':
                    //    val.Append(ch);
                    //    break;
                    //case 'f':
                    //    val.Append(ch);
                    //    break;
                    //case 'F':
                    //    val.Append(ch);
                    //    break;
                    //case 'l':
                    //    val.Append(ch);
                    //    break;
                    //case 'L':
                    //    val.Append(ch);
                    //    break;
                    //case 'u':
                    //    val.Append(ch);
                    //    break;
                    //case 'U':
                    //    val.Append(ch);
                    //    break;
                    case '.':
                        if(val.ToString().IndexOf('.')>=0) return new EvalObject() { Type = EvalObjectType.Error, Value = "Error Repeat Char " + ch };
                        val.Append(ch);
                        break;
                    //case ':':
                    //    if (!val.ToString().EndsWith(".")) val.Clear();
                    //    break;

                    default:
                        {
                            //函数
                            if(ch>='a' && ch<='z' || ch>='A' && ch <= 'Z')
                            {
                                fun.Append(ch);
                                break;
                            }
                            //推入参数
                            else if (stf.Count>0 && ch==',')
                            {
                                if (val.Length > 0)
                                {
                                    //Console.WriteLine("推入参数：{0}", val.ToString());
                                    stf.Peek().Add(val.ToString());
                                    val.Clear();
                                    break;
                                }
                                else
                                {
                                    return new EvalObject() { Type = EvalObjectType.Error, Value = "Error Function Parameters Can't null or empty" };
                                }
                            }
                        }

                        return new EvalObject() {  Type= EvalObjectType.Error,Value="Error Non Supers Char "+ ch };
                }
            }


            //Console.WriteLine("=堆栈结束");
            if (val.Length > 0)
            {
                pushB(val, que);
            }
            js2(st, que, OperatorType.Empty);

            //Console.WriteLine("=转换结束");

            //错误值
            return que.Dequeue();
        }

        /// <summary>
        /// 二目 计算
        /// </summary>
        /// <param name="st">堆栈</param>
        /// <param name="que">队列</param>
        /// <param name="opt">运算符</param>
        private static void js2(Stack<EvalObject> st, Queue<EvalObject> que, OperatorType opt)
        {
            //Console.WriteLine("==计算数据："+ st.ToJsonString());

            if (st.Count == 0)
            {
                return;
            }

            if (st.Peek().Type != EvalObjectType.Operator) return;

            var op = (OperatorType)st.Peek().Value;
            //之前是乘除
            if (op == OperatorType.Factorial || op == OperatorType.Factorial2)
            {
                st.Pop();
                var A = que.Dequeue();
                var C = eval(A, op);
                que.Enqueue(C);
                js2(st, que, opt);
            }
            //之前是乘除
            else if (op == OperatorType.Mul || op == OperatorType.Div || op == OperatorType.Mod)
            {
                st.Pop();
                var A = st.Pop();
                var B = que.Dequeue();
                var C = eval(A, B, op);
                que.Enqueue(C);
                js2(st, que, opt);
            }
            //现在乘除 之前加减
            else if ((opt == OperatorType.Mul || opt == OperatorType.Div || opt == OperatorType.Mod) &&(op == OperatorType.Add || op == OperatorType.Sub))
            {
            }
            //现在加减 之前加减
            else if ((opt == OperatorType.Add || opt == OperatorType.Sub || opt== OperatorType.Empty) && (op == OperatorType.Add || op == OperatorType.Sub))
            {
                st.Pop();
                var A = st.Pop();
                var B = que.Dequeue();
                var C = eval(A, B, op);
                que.Enqueue(C);
            }
            //右括直到遇到左括 右函数左括
            else if (opt == OperatorType.BracketRight && op!= OperatorType.BracketLeft && op!=OperatorType.FunctionLeft)
            {
                st.Pop();
                var A = st.Pop();
                var B = que.Dequeue();
                var C = eval(A, B, op);
                que.Enqueue(C);
                js2(st, que, opt);
            }
            //左括弹出 这个在上
            else if (opt == OperatorType.BracketRight && op == OperatorType.BracketLeft)
            {
                st.Pop();
            }
            //左括 退出 不计算
            else if (op == OperatorType.BracketLeft)
            {
                return;
            }
            else
            {
                st.Pop();
                var A = st.Pop();
                var B = que.Dequeue();
                var C = eval(A, B, op);
                que.Enqueue(C);
                js2(st, que, opt);
            }
        }

        /// <summary>
        /// 从队列推入到栈
        /// </summary>
        /// <param name="que"></param>
        /// <param name="st"></param>
        private static void pushB(Queue<EvalObject> que, Stack<object> st)
        {
            if (que.Count > 0)
            {
                var B= que.Dequeue();
                st.Push(B);
            }
        }

        /// <summary>
        /// 推入队列
        /// </summary>
        /// <param name="val"></param>
        /// <param name="que"></param>
        private static void pushB(StringBuilder val, Queue<EvalObject> que)
        {
            if (val.Length > 0)
            {
                //var B = ParseVal(val.ToString());
                var B = ParseBigVal(val.ToString());
                if (typeof(string) == B.GetType()) throw new Exception("这里解析的值不能是string,否则会导致死循环");
                que.Enqueue(new EvalObject(B));
                val.Clear();
            }
        }

        #endregion

        #region "一元运算"
        /// <summary>
        /// 各种数据类型
        /// 只转换成四种数据类型 ulong long doble decimal
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static EvalObject eval(this object A)
        {
            return new EvalObject(A);
        }

        /// <summary>
        /// 单目计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="operator"></param>
        /// <returns></returns>
        public static EvalObject eval(this EvalObject a, OperatorType @operator)
        {
            switch (a.Type)
            {
                case EvalObjectType.Int64:
                    return ((long)a.Value).eval(@operator);
                case EvalObjectType.UInt64:
                    return ((ulong)a.Value).eval(@operator);
                case EvalObjectType.BigInteger:
                    return ((BigInteger)a.Value).eval(@operator);
                default:
                    throw new Exception(a.Type+"此类型，不支持单目运算");
            }
        }

        /// <summary>
        /// 单目计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="operator"></param>
        /// <returns></returns>
        public static long eval(this long a, OperatorType @operator)
        {
            switch (@operator)
            {
                case OperatorType.Increment:
                    return a++;
                case OperatorType.Decrement:
                    return a--;
                case OperatorType.Add:
                    return a;
                case OperatorType.Sub:
                    return -a;
                case OperatorType.Factorial:
                    return Factorial(a);
                case OperatorType.Factorial2:
                    return Factorial2(a);
                default:
                    throw new Exception("不支持此单目运算符" + @operator);
            }
        }

        /// <summary>
        /// 单目计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="operator"></param>
        /// <returns></returns>
        public static ulong eval(this ulong a, OperatorType @operator)
        {
            switch (@operator)
            {
                case OperatorType.Increment:
                    return a++;
                case OperatorType.Decrement:
                    return a--;
                case OperatorType.Add:
                    return a;
                case OperatorType.Factorial:
                    return Factorial(a);
                case OperatorType.Factorial2:
                    return Factorial2(a);
                default:
                    throw new Exception("不支持此单目运算符" + @operator);
            }
        }


        /// <summary>
        /// 单目计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="operator"></param>
        /// <returns></returns>
        public static BigInteger eval(this BigInteger a, OperatorType @operator)
        {
            switch (@operator)
            {
                case OperatorType.Increment:
                    return a++;
                case OperatorType.Decrement:
                    return a--;
                case OperatorType.Add:
                    return a;
                case OperatorType.Sub:
                    return -a;
                case OperatorType.Factorial:
                    return Factorial(a);
                case OperatorType.Factorial2:
                    return Factorial2(a);
                default:
                    throw new Exception("不支持此单目运算符" + @operator);
            }
        }

        private static long Factorial2(long a)
        {
            return factorial(a, 2L);
        }

        private static long Factorial(long a)
        {
            return factorial(a, 1L);
        }

        private static long factorial(long bl, long jc)
        {
            long br = 1L;
            if (Math.Abs(bl) / jc > 999) throw new Exception("数值太大，拒绝阶乘运算，"+bl);
            if (bl > 0)
            {
                for (var fi = bl; fi > 0L; fi -= jc)
                {
                    br *= fi;
                }
            }
            else if (bl < 0)
            {
                for (var fi = bl; fi < 0L; fi += jc)
                {
                    br *= fi;
                }
            }
            else
            {
                return 0;
            }
            return br;
        }

        private static ulong Factorial2(ulong a)
        {
            return factorial(a, 2ul);
        }

        private static ulong Factorial(ulong a)
        {
            return factorial(a, 1ul);
        }

        private static ulong factorial(ulong bl, ulong jc)
        {
            ulong br = 1L;
            if (bl / jc > 999) throw new Exception("数值太大，拒绝阶乘运算，"+bl);
            if (bl > 0)
            {
                for (var fi = bl; fi > 0L; fi -= jc)
                {
                    br *= fi;
                }
            }
            else if (bl < 0)
            {
                for (var fi = bl; fi < 0L; fi += jc)
                {
                    br *= fi;
                }
            }
            else
            {
                return 0;
            }
            return br;
        }

        private static BigInteger Factorial2(BigInteger a)
        {
            return factorial(a, 2);
        }

        private static BigInteger factorial(BigInteger bl, int jc)
        {
            BigInteger br = 1L;
            if ((BigInteger.Abs(bl) / jc) > 999) throw new Exception("数值太大，拒绝阶乘运算，"+ bl);
            if (bl > 0)
            {
                for (var fi = bl; fi > 0L; fi -= jc)
                {
                    br *= fi;
                }
            }
            else if (bl < 0)
            {
                for (var fi = bl; fi < 0L; fi += jc)
                {
                    br *= fi;
                }
            }
            else
            {
                return 0;
            }
            return br;
        }

        private static BigInteger Factorial(BigInteger a)
        {
            return factorial(a, 1);
        }
        #endregion

        #region "二元定型运算"

        /// <summary>
        /// 已知类型计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="operator"></param>
        /// <returns></returns>
        public static ulong eval(this ulong a, ulong b,OperatorType @operator)
        {
            switch (@operator)
            {
                case OperatorType.Add:
                    return a + b;
                case OperatorType.Sub:
                    return a - b;
                case OperatorType.Mul:
                    return a * b;
                case OperatorType.Div:
                    return a / b;
                case OperatorType.Mod:
                    return a % b;
                case OperatorType.Min:
                    return Math.Min(a,b) ;
                case OperatorType.Max:
                    return Math.Max(a, b);
            }

            throw new Exception("未知的运算符"+ @operator);
        }

        /// <summary>
        /// 已知类型计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="operator"></param>
        /// <returns></returns>
        public static long eval(this long a, long b, OperatorType @operator)
        {
            switch (@operator)
            {
                case OperatorType.Add:
                    return a + b;
                case OperatorType.Sub:
                    return a - b;
                case OperatorType.Mul:
                    return a * b;
                case OperatorType.Div:
                    return a / b;
                case OperatorType.Mod:
                    return a % b;
                case OperatorType.Min:
                    return Math.Min(a, b);
                case OperatorType.Max:
                    return Math.Max(a, b);
            }

            throw new Exception("未知的运算符" + @operator);
        }

        /// <summary>
        /// 已知类型计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="operator"></param>
        /// <returns></returns>
        public static double eval(this double a, double b, OperatorType @operator)
        {
            switch (@operator)
            {
                case OperatorType.Add:
                    return a + b;
                case OperatorType.Sub:
                    return a - b;
                case OperatorType.Mul:
                    return a * b;
                case OperatorType.Div:
                    return a / b;
                case OperatorType.Mod:
                    return a % b;
                case OperatorType.Min:
                    return Math.Min(a, b);
                case OperatorType.Max:
                    return Math.Max(a, b);
            }

            throw new Exception("未知的运算符" + @operator);
        }

        /// <summary>
        /// 已知类型计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="operator"></param>
        /// <returns></returns>
        public static decimal eval(this decimal a, decimal b, OperatorType @operator)
        {
            switch (@operator)
            {
                case OperatorType.Add:
                    return a + b;
                case OperatorType.Sub:
                    return a - b;
                case OperatorType.Mul:
                    return a * b;
                case OperatorType.Div:
                    return a / b;
                case OperatorType.Mod:
                    return a % b;
                case OperatorType.Min:
                    return Math.Min(a, b);
                case OperatorType.Max:
                    return Math.Max(a, b);
            }

            throw new Exception("未知的运算符" + @operator);
        }

        /// <summary>
        /// 已知类型计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="operator"></param>
        /// <returns></returns>
        public static BigInteger eval(this BigInteger a, BigInteger b, OperatorType @operator)
        {
            switch (@operator)
            {
                case OperatorType.Add:
                    return a + b;
                case OperatorType.Sub:
                    return a - b;
                case OperatorType.Mul:
                    return a * b;
                case OperatorType.Div:
                    return a / b;
                case OperatorType.Mod:
                    return a % b;
                case OperatorType.Min:                    
                    return BigInteger.Min(a, b);
                case OperatorType.Max:
                    return BigInteger.Max(a, b);
            }

            throw new Exception("未知的运算符" + @operator);
        }


        /// <summary>
        /// 已知类型计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="operator"></param>
        /// <returns></returns>
        public static BigDecimal eval(this BigDecimal a, BigDecimal b, OperatorType @operator)
        {
            switch (@operator)
            {
                case OperatorType.Add:
                    return a + b;
                case OperatorType.Sub:
                    return a - b;
                case OperatorType.Mul:
                    return a * b;
                case OperatorType.Div:
                    return a / b;
                case OperatorType.Mod:
                    return a % b;
                case OperatorType.Min:
                    return BigDecimal.Min(a, b);
                     //BigDecimal.Min(a, b);
                case OperatorType.Max:
                    return BigDecimal.Max(a, b);
                    //return BigDecimal.Max(a, b);
            }

            throw new Exception("未知的运算符" + @operator);
        }
        #endregion

        #region "二元泛型运算"

        /// <summary>
        /// 已知类型计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="operator"></param>
        /// <returns></returns>
        public static EvalObject eval(this EvalObject a, EvalObject b, OperatorType @operator)
        {
            //Console.WriteLine("转换前：{3}-->{0}{1}{2}", a.ToJsonString(), @operator, b.ToJsonString(), maxType.ToString());
            var maxType = (EvalObjectType)Enum.Parse(typeof(EvalObjectType), Math.Max((int)a.Type, (int)b.Type).ToString());

            if (a.Type == b.Type)
            {
                switch (maxType)
                {
                    case EvalObjectType.BigDecimal:
                        return eval((BigDecimal)a.Value, (BigDecimal)b.Value, @operator);
                    case EvalObjectType.BigInteger:
                        return eval((BigInteger)a.Value, (BigInteger)b.Value, @operator);
                    case EvalObjectType.Decimal:
                        return eval((decimal)a.Value, (decimal)b.Value, @operator);
                    case EvalObjectType.Double:
                        return eval((double)a.Value, (double)b.Value, @operator);
                    case EvalObjectType.Int64:
                        return eval((long)a.Value, (long)b.Value, @operator);
                    case EvalObjectType.UInt64:
                        return eval((ulong)a.Value, (ulong)b.Value, @operator);
                    default:
                        throw new Exception(a.Type + "此类型无法计算");
                }
            }
            else
            {
                switch (maxType)
                {
                    case EvalObjectType.BigDecimal:
                        return eval((EvalObject<BigDecimal>)a, (EvalObject<BigDecimal>)b, @operator).Value;
                    case EvalObjectType.BigInteger:
                        return eval((EvalObject<BigInteger>)a, (EvalObject<BigInteger>)b, @operator).Value;
                    case EvalObjectType.Decimal:
                        return eval((EvalObject<decimal>)a, (EvalObject<decimal>)b, @operator).Value;
                    case EvalObjectType.Double:
                        return eval((EvalObject<double>)a, (EvalObject<double>)b, @operator).Value;
                    case EvalObjectType.Int64:
                        return eval((EvalObject<long>)a, (EvalObject<long>)b, @operator).Value;
                    case EvalObjectType.UInt64:
                        return eval((EvalObject<ulong>)a, (EvalObject<ulong>)b, @operator).Value;
                    default:
                        throw new Exception(a.Type + "此类型无法计算");
                }
            }

            throw new Exception("未知的运算符" + @operator);
        }

        /// <summary>
        /// 已知类型计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="operator"></param>
        /// <returns></returns>
        public static EvalObject<T> eval<T>(this EvalObject<T> a, EvalObject<T> b, OperatorType @operator)
        {
            //Console.WriteLine("转换后：{0}{1}{2}", a.ToJsonString(), @operator, b.ToJsonString());
            switch (a.Type)
            {
                case EvalObjectType.BigDecimal:
                    return eval((BigDecimal)(object)a.Value, (BigDecimal)(object)b.Value, @operator);
                case EvalObjectType.BigInteger:
                    return eval((BigInteger)(object)a.Value, (BigInteger)(object)b.Value, @operator);
                case EvalObjectType.Decimal:
                    return eval((decimal)(object)a.Value, (decimal)(object)b.Value, @operator);
                case EvalObjectType.Double:
                    return eval((double)(object)a.Value, (double)(object)b.Value, @operator);
                case EvalObjectType.Int64:
                    return eval((long)(object)a.Value, (long)(object)b.Value, @operator);
                case EvalObjectType.UInt64:
                    return eval((ulong)(object)a.Value, (ulong)(object)b.Value, @operator);
                default:
                    throw new Exception(a.Type+"此类型无法计算");
            }
        }

        #endregion
    }

}

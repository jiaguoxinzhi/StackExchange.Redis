using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Numerics;

namespace WS_Core.dyCompiler
{
    /// <summary>
    /// 大值 十进制
    /// 实际上有官方实现<see cref="BigInteger"/> 性能更带有优势
    /// </summary>
    [Author("Linyee", "2019-02-11")]
    //经比较解析值先用*10，uint位计算，然后各种算法依靠原有二进制，，的以快很多。。
    public struct BigDec
    {

        #region "参数"

        ///// <summary>
        ///// 模值
        ///// </summary>
        //private byte Modulus { get { return _Modulus; } set { _Modulus = value; BitMax = (byte)(_Modulus - 1); } }
        internal const byte _Modulus = 10;

        /// <summary>
        /// 每位 最大值
        /// </summary>
        internal const byte BitMax = 9;

        /// <summary>
        /// 每位 最小值
        /// </summary>
        internal const byte BitMin = 0;
        /// <summary>
        /// 0
        /// </summary>
        public static readonly BigDec Zero=new BigDec();

        /// <summary>
        /// 无效值
        /// </summary>
        internal static readonly BigDec NaN=new BigDec() {
            Data=new byte[0],
        };
        #endregion

        #region "判断"
        /// <summary>
        /// 是否为无效值
        /// </summary>
        internal bool IsNaN=> Data==null || Data.LongLength<0;

        /// <summary>
        /// 是否正数
        /// </summary>
        internal bool SignIsBitMin => Sign == BitMin;
        /// <summary>
        /// 是否负数
        /// </summary>
        internal bool SignIsBitMax => Sign == BitMax;

        /// <summary>
        /// 值是否为0
        /// </summary>
        public bool IsZero
        {
            get
            {
                if (Sign == BitMax) return false;

                for (var fi = 0; fi < LongLength; fi++)
                {
                    if (Data[fi] > 0) return false;
                }

                return true;
            }
        }
        #endregion

        #region "属性 字段"
        /// <summary>
        /// 大值
        /// </summary>
        private BigDec(long len) : this((byte)10)
        {
            Data = new byte[(len/16+((len%16)>0?1:0))*16];
        }

        /// <summary>
        /// 大值
        /// </summary>
        private BigDec(byte modulus) : this()
        {
            Data = new byte[16];
            //Modulus = modulus;
        }

        /// <summary>
        /// 值数据
        /// </summary>
        private byte[] Data;

        /// <summary>
        /// 符号位
        /// </summary>
        internal byte Sign => Data[Data.Length - 1];

        /// <summary>
        /// 长度
        /// </summary>
        public int Length => Data.Length;

        /// <summary>
        /// 长度
        /// </summary>
        public long LongLength => Data.LongLength;

        /// <summary>
        /// 转字符串补码
        /// </summary>
        /// <param name="type">0默认方案 1自动收缩 4格式化</param>
        /// <returns></returns>
        public string ToBCDString(int type=1)
        {
            StringBuilder sbd = new StringBuilder();
            for(var fi = 1; fi <= LongLength; fi++)
            {
                sbd.Append(Data[LongLength-fi]);
            }
            var res = sbd.ToString();
            sbd.Clear();

            //1自动收缩
            if ((type & 1)>0)
            {
                res= res.TrimStart('0');
                if (string.IsNullOrEmpty(res)) res= "0";//0时
            }

            //4格式化
            if ((type & 4) > 0)
            {
                res = res.TrimStart('0');
                if (string.IsNullOrEmpty(res)) res = "0";//0时

                if (res.Length>4)
                {
                    var rev = res.Reverse().ToArray();
                    for (var fi = 0; fi < rev.LongLength; fi++)
                    {
                        sbd.Append(rev[fi]);
                        if ((fi + 1) % 4 == 0) sbd.Append(",");
                    }
                    res = string.Join("", sbd.ToString().Reverse()).TrimStart(',');
                }
            }

            return res;
        }

        /// <summary>
        /// 预定义格式
        /// </summary>
        /// <param name="type">0默认方案 1自动收缩 2带符号 4格式化</param>
        /// <returns></returns>
        public string ToString(int type)
        {
            if (Data.LongLength < 1) return "NaN";

            //符号处理
            var sign = "";
            var stype = (type & 0x2);
            if((stype & 2) >0){
                if (SignIsBitMax) sign = "-";
                else sign = "+";
            }
            else
            {
                if (SignIsBitMax) sign = "-";
            }

            //值处理
            if (Sign == BitMin)
            {
                return sign + ToBCDString(type & (0x1 | 0x4));
            }
            else
            {
                var val = this.Clone();
                --val;
                //Console.WriteLine("负号输出自减后："+ val.ToBCDString());
                val = ~val;
                //Console.WriteLine("负号输出取反后：" + val.ToBCDString());
                return sign + val.ToBCDString(type & (0x1 | 0x4));
            }
        }

        /// <summary>
        /// 转字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(5);
        }
        #endregion

        #region "转入"
        /// <summary>
        /// 转入
        /// </summary>
        /// <param name="text"></param>
        public static implicit operator BigDec(string text)
        {
            if (text == "NaN") return NaN;
            if (string.IsNullOrEmpty(text)) return Zero;

            var val = Parse(text);
            return val;
        }

        /// <summary>
        /// 解析
        /// 可解析正负数
        /// </summary>
        /// <param name="aa"></param>
        /// <returns></returns>
        public static BigDec Parse(string aa)
        {
            if (string.IsNullOrEmpty(aa)) return Zero;
            var a = ExpressionEval.spaceBodyRegex.Replace(aa, newEmpty);
            if (ExpressionEval.lngRegex.IsMatch(a))
            {
                bool isfu = false;
                if (a.Substring(0, 1) == "-") isfu = true;
                var str = ExpressionEval.lngBodyOnlyRegex.Match(a).Value;
                var val = ParseLong(str);
                if (isfu) val = -val;
                return val;
            }
            else
            {
                throw new Exception("不是一个有效的数值串");
            }
        }

        /// <summary>
        /// 替换为空
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private static string newEmpty(Match match)
        {
            //Console.Write(match.Value);
            return "";
        }

        /// <summary>
        /// 从数值串解析
        /// 只解析正数
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private static BigDec ParseLong(string a)
        {
            //Console.WriteLine("长度：" + a.Length);
            var val = new BigDec(a.Length + 1);
            var rev = a.Reverse().ToArray();
            for (var fi = 0; fi < val.LongLength; fi++)
            {
                if (fi + 1 > rev.LongLength)
                {
                    val.Data[fi] = 0;
                }
                else
                {
                    val.Data[fi] = (byte)int.Parse("" + rev[fi]);
                }
            }
            return val;
        }


        /// <summary>
        /// 转入
        /// </summary>
        /// <param name="A"></param>
        public static implicit operator BigDec(int A)
        {
            var val = A.ToString();
            return ParseLong(val);
        }


        /// <summary>
        /// 转入
        /// </summary>
        /// <param name="A"></param>
        public static implicit operator BigDec(uint A)
        {
            var val = A.ToString();
            return ParseLong(val);
        }

        /// <summary>
        /// 转入
        /// </summary>
        /// <param name="A"></param>
        public static implicit operator BigDec(long A)
        {
            var val = A.ToString();
            return ParseLong(val);
        }

        /// <summary>
        /// 转入
        /// </summary>
        /// <param name="A"></param>
        public static implicit operator BigDec(ulong A)
        {
            var val = A.ToString();
            return ParseLong(val);
        }


        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="A"></param>
        public static BigDec CreateFrom(object A)
        {
            var atype = A.GetType();
            if (atype == typeof(BigDec)) return (BigDec)A;
            var acode = Convert.GetTypeCode(A);
            switch (acode)
            {
                case TypeCode.Byte:
                    return (BigDec)(long)(byte)A;
                case TypeCode.Int16:
                    return (BigDec)(long)(Int16)A;
                case TypeCode.Int32:
                    return (BigDec)(long)(Int32)A;
                case TypeCode.Int64:
                    return (BigDec)(long)(Int64)A;
                case TypeCode.SByte:
                    return (BigDec)(long)(SByte)A;
                case TypeCode.UInt16:
                    return (BigDec)(ulong)(UInt16)A;
                case TypeCode.UInt32:
                    return (BigDec)(ulong)(UInt32)A;
                case TypeCode.UInt64:
                    return (BigDec)(ulong)(UInt64)A;
                case TypeCode.Boolean:
                    return ((bool)A)?1:0;
                case TypeCode.Char:
                case TypeCode.String:
                    return Parse(A.ToString());
                case TypeCode.DateTime:
                    return ((DateTime)A).Ticks;
                default:
                    throw new Exception(string.Format("不支持{0}类型转为BigDec", atype));
            }
        }
        #endregion

        #region "一元"

        #region "一元位运算符"
        /// <summary>
        /// 按位与运算
        /// 不影响原值
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static BigDec operator ~(BigDec A)
        {
            var R = A.Clone();
            for (var fi = 0; fi < R.Data.Length; fi++)
            {
                R.Data[fi] = (byte)(BitMax - R.Data[fi]);//按位取反
            }
            return R;
        }

        /// <summary>
        /// 克隆一个值
        /// 使之不影响源数
        /// </summary>
        /// <returns></returns>
        public BigDec Clone()
        {
            var val= new BigDec();
            val.Data = new byte[this.LongLength];
            Array.Copy(Data, val.Data, val.LongLength);
            return val;
        }

        #endregion

        #region "一元运算符"
        /// <summary>
        /// 自增
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static BigDec operator ++(BigDec A)
        {
            //Console.WriteLine("源数："+A.ToBCDString());
            int jin = 1;
            for (var fi = 0; fi < A.Data.Length; fi++)
            {
                if (jin < 1) break;
                var sum = A.Data[fi] + jin;
                A.Data[fi] =(byte) (sum % _Modulus);//按位取模
                jin = sum / _Modulus;//进数
            }
            //Console.WriteLine("结果：" + A.ToBCDString());
            return A;
        }

        /// <summary>
        /// 自减
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static BigDec operator --(BigDec A)
        {
            int jin = -1;
            for (var fi = 0; fi < A.Data.Length; fi++)
            {
                if (jin > -1) break;
                if (A.Data[fi] > 0)
                {
                    var sum = A.Data[fi] + jin;
                    A.Data[fi] = (byte)Math.Abs(sum % _Modulus);//按位取模

                    jin = 0;
                }
                else
                {
                    var sum = _Modulus + jin;
                    A.Data[fi] = (byte)Math.Abs(sum % _Modulus);//按位取模
                    jin = -1;
                }
            }
            return A;
        }
        
        /// <summary>
        /// 正号
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static BigDec operator +(BigDec A)
        {
            return A.Clone();
        }

        /// <summary>
        /// 负号
        /// 不影响原值
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static BigDec operator -(BigDec A)
        {
            if (A.IsZero) return A;//0 返回自身

            var R = A.Clone();
            //Console.WriteLine("负号源数：" + R.ToBCDString());
            R = ~R;//先取反
            //Console.WriteLine("负号取反：" + R.ToBCDString());
            R++;//再加1
            //Console.WriteLine("负号结果：" + R.ToBCDString());
            return R;
        }


        #endregion


        #endregion

        #region "二元"

        #region "二元位运算符"
        /// <summary>
        /// 按位右移
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static BigDec operator >>(BigDec A, int B)
        {
            var R = A.Clone();
            for (var fi = 0; fi < R.LongLength; fi++)
            {
                if (fi + B < R.LongLength - 1)
                {
                    R.Data[fi] = R.Data[fi + B];//右移
                }
                else
                {
                    R.Data[fi] = R.Sign;//右移
                }
            }
            return R;
        }

        /// <summary>
        /// 按位左移
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static BigDec operator <<(BigDec A, int B)
        {
            var R = A.Clone();
            if (!R.HighIsSign(B)) R.Expand(B);//扩充位数
            for (var fi = 0; fi < R.LongLength - 1; fi++)
            {
                if (R.LongLength - 2 - fi - B < 0)
                {
                    R.Data[R.LongLength - 2 - fi] = 0;//补0
                }
                else
                {
                    R.Data[R.LongLength - 2 - fi] = R.Data[R.LongLength - 2 - fi - B];
                }
            }
            return R;
        }

        /// <summary>
        /// 扩充位数
        /// 任意倍数自动转8倍数
        /// </summary>
        /// <param name="b"></param>
        private BigDec Expand(long b)
        {
            if (b < 0) throw new Exception("扩充位数不能负数");

            if (b <= 8)
            {
                Expand();
            }
            else
            {
                Expand((ulong)((b / 8 + (b % 8) > 0 ? 1 : 0) * 8));
            }
            return this;
        }


        /// <summary>
        /// 扩充位数
        /// 扩充位数必须是8的倍数
        /// </summary>
        private BigDec Expand(ulong len = 8)
        {
            if ((len % 8) > 0) throw new Exception("扩充位数必须是8的倍数");

            List<byte> list = new List<byte>();
            list.AddRange(Data);
            var vh = new byte[len];
            for (var fi = 0; fi < vh.Length; fi++)
            {
                vh[fi] = Sign;
            }
            list.AddRange(Data);
            Data = list.ToArray();
            return this;
        }

        /// <summary>
        /// 收缩位数
        /// 高8位都是符号值时 一次收缩8位 直到只有16位
        /// </summary>
        private BigDec Shrink()
        {
            if (HighIsSign(8) && Data.Length >= 24)
            {
                var data = new byte[Data.Length - 8];
                Array.Copy(Data, data, data.Length);
                Data = data;
                Shrink();
            }

            return this;
        }

        /// <summary>
        /// 高几位是否都是符号值
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool HighIsSign(int b)
        {
            for (var fi = 0; fi < b; fi++)
            {
                var val = Data[Data.Length - 2 - fi];
                if (val != Sign) return false;
            }
            return true;
        }
        #endregion

        #region "二元运算符"

        /// <summary>
        /// 加法
        /// </summary>
        /// <param name="text"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static string operator +(string text, BigDec B)
        {
            return text+B.ToString();
        }

        /// <summary>
        /// 加法
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static BigDec operator +(BigDec A, BigDec B)
        {
            //检测
            A.CheckSign();
            B.CheckSign();

            //结果数
            var maxlen = Math.Max(A.LongLength, B.LongLength);
            var C = new BigDec() {
                Data = new byte[maxlen],
            };
            C.Expand();//扩充

            //计算
            byte jin = 0;
            for (var fi=0; fi< C.LongLength; fi++)
            {
                var va = A.Sign;
                var vb = B.Sign;
                if (fi < A.LongLength) va = A.Data[fi];
                if (fi < B.LongLength) vb = B.Data[fi];

                var sum = va + vb + jin;
                //Console.WriteLine("加法结果：{4} {0}+{1}+{2}={3}", va, vb, jin, sum, fi);
                jin =(byte) (sum / _Modulus);
                C.Data[fi] = (byte)(sum%_Modulus);
            }
            //判断结果
            C.CheckOverflow();
            C.Shrink();//收缩

            //Console.WriteLine("加法结果：{0}+{1}={2} {3}",A.ToBCDString(1),B.ToBCDString(1),C.ToBCDString(1),B.Sign);
            return C;
        }

        /// <summary>
        /// 检测符号位
        /// </summary>
        private BigDec CheckSign()
        {
            if (Sign != BitMin && Sign != BitMax) throw new Exception("不是一个有效的有符数据");
            return this;
        }

        /// <summary>
        /// 检测溢出
        /// </summary>
        private BigDec CheckOverflow()
        {
            if (Sign != BitMin && Sign != BitMax) throw new Exception("计算溢出");
            return this;
        }

        /// <summary>
        /// 减法
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static BigDec operator -(BigDec A, BigDec B)
        {
            //检测
            A.CheckSign();
            B.CheckSign();
            var BB = -B;
            //Console.WriteLine("-- {0}+{1}", A.ToBCDString(), (-B).ToBCDString());
            var C= A + BB;
            //Console.WriteLine("减法结果：" + C.ToBCDString());
            return C;
        }

        
        /// <summary>
        /// 乘法
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static BigDec operator *(BigDec A, BigDec B)
        {
            //检测
            A.CheckSign();
            B.CheckSign();

            var AA = A.Clone();
            var BB = B.Clone();
            if (AA.SignIsBitMax) AA = -AA;
            if (BB.SignIsBitMax) BB = -BB;
            AA.Shrink();
            BB.Shrink();

            //结果数
            var maxlen = AA.LongLength+ BB.LongLength;
            var C = new BigDec(maxlen);
            C.Expand();//扩充

            //计算
            for (var fi = 0; fi < BB.LongLength; fi++)
            {
                var CC = Mul(AA, BB.Data[fi], fi);
                C = C + CC;
            }

            if (A.Sign != B.Sign) C = -C;

            //判断结果
            C.CheckOverflow();
            C.Shrink();//收缩

            //Console.WriteLine("加法结果：{0}+{1}={2} {3}",A.ToBCDString(1),B.ToBCDString(1),C.ToBCDString(1),B.Sign);
            return C;
        }

        /// <summary>
        /// 多对1 乘法
        /// 只支持正数
        /// </summary>
        /// <param name="a"></param>
        /// <param name="v"></param>
        /// <param name="ei">添加后缀几个0</param>
        /// <returns></returns>
        private static BigDec Mul(BigDec a, byte v, int ei)
        {
            if (a.SignIsBitMax) throw new Exception("只支持正数");

            BigDec A = null;
            var ends = (ei > 0 ? new string('0', ei) : "");
            A = a.Clone().ToString()+ ends;

            BigDec C = new BigDec(A.LongLength+1);
            for (var fi = 0; fi < A.LongLength; fi++)
            {
                var product = A.Data[fi] * v;
                C.Data[fi + 1] += (byte)(product / _Modulus);
                C.Data[fi] += (byte)(product % _Modulus);
            }

            return C;
        }


        /// <summary>
        /// 乘法
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static BigDecFen operator /(BigDec A, BigDec B)
        {
            //检测
            A.CheckSign();
            B.CheckSign();

            BigDecFen C =null;
            var AA = A.Abs();
            var BA = B.Abs();

            if (AA < BA)
            {
                //Console.WriteLine("===小于");
                C = new BigDecFen(0, AA, BA);
            }else if (AA==BA)
            {
                //Console.WriteLine("===等于");
                C = new BigDecFen(1, 0, BA);
            }
            else if (AA.IsZero)
            {
                //Console.WriteLine("===零除");
                C = new BigDecFen(0, 0, BA);
            }
            else if (BA.IsZero)
            {
                //Console.WriteLine("===除零");
                C = BigDecFen.NaN;
            }
            else
            {
                //Console.WriteLine("===其它");
                C = new BigDecFen(0, AA, BA);
            }

            if (A.Sign != B.Sign) C = -C;

            return C;
        }

        /// <summary>
        /// 绝对值
        /// </summary>
        /// <returns></returns>
        public BigDec Abs()
        {
            if (SignIsBitMin) return this;
            else return -this;
        }

        #endregion

        #region "逻辑比较 除法必须用"
        /// <summary>
        /// 大于
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator >(BigDec A, BigDec B)
        {
            return Compare(A,B)==1;
        }
        /// <summary>
        /// 小于
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator <(BigDec A, BigDec B)
        {
            var c = Compare(A, B);
            //Console.WriteLine("比较结果："+c);
            return c== -1;
        }
        /// <summary>
        /// 大于
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator >=(BigDec A, BigDec B)
        {
            var c = Compare(A, B);
            return  c>= 0;
        }
        /// <summary>
        /// 小于
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator <=(BigDec A, BigDec B)
        {
            return Compare(A, B) <= 0;
        }
        /// <summary>
        /// 大于
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator ==(BigDec A, BigDec B)
        {
            return Compare(A, B) == 0;
        }
        /// <summary>
        /// 小于
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator !=(BigDec A, BigDec B)
        {
            return Compare(A, B) != 0;
        }

        /// <summary>
        /// 标识码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// 比较
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            BigDec B = CreateFrom(obj);
            return this == B;
        }

        /// <summary>
        /// 比较
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static int Compare(BigDec A, BigDec B)
        {
            var AA = A.Clone();
            var BB = B.Clone();
            AA.Shrink();
            BB.Shrink();

            if (AA.SignIsBitMin && BB.SignIsBitMax)
            {
                return 1;
            }
            else if (AA.SignIsBitMax && BB.SignIsBitMin)
            {
                //Console.WriteLine("A负B正：-1" );
                return -1;
            }
            else if ((AA.SignIsBitMin && BB.SignIsBitMin) || (AA.SignIsBitMax && BB.SignIsBitMax))
            {
                var mlen = Math.Max(AA.LongLength, BB.LongLength);
                for (var fi = 0; fi < mlen; fi++)
                {
                    byte va = 0;
                    byte vb = 0;

                    var vi = mlen - 1 - fi;
                    if (vi < AA.LongLength) va = AA.Data[vi];
                    if (vi < BB.LongLength) vb = BB.Data[vi];
                    //Console.WriteLine("{2}:{0}loga{1}",va,vb, vi);

                    if (va > vb) return 1;
                    if (va < vb) return -1;
                }
                return 0;
            }
            else
            {
                throw new Exception("不可能存在的情况");
            }
        }
        #endregion

        #endregion

    }

    /// <summary>
    /// 分数表示
    /// 分子必须小于分母
    /// </summary>
    [Author("Linyee", "2019-02-13")]
    public struct BigDecFen
    {
        /// <summary>
        /// 0值
        /// </summary>
        public static readonly BigDecFen ZeroZeroOne = new BigDecFen(0,0,1);

        /// <summary>
        /// 无效值
        /// </summary>
        public static readonly BigDecFen NaN = new BigDecFen() {
            Quotient = BigDec.NaN,
            Remainder = BigDec.NaN,
            Denominator = BigDec.NaN,
            Sign = 1,
        };

        /// <summary>
        /// 圆周率精确到6位
        /// </summary>
        public static readonly BigDecFen PI = new BigDecFen(3, 16, 113);


        /// <summary>
        /// 商数
        /// </summary>
        private BigDec Quotient;
        /// <summary>
        /// 余数
        /// </summary>
        private BigDec Remainder;
        /// <summary>
        /// 分母
        /// </summary>
        private BigDec Denominator;

        /// <summary>
        /// 符号
        /// </summary>
        private byte Sign;

        /// <summary>
        /// 分数
        /// </summary>
        /// <param name="Q">商数</param>
        /// <param name="R">余数</param>
        /// <param name="D">分母</param>
        public BigDecFen(BigDec Q, BigDec R, BigDec D) : this()
        {
            if (Q.SignIsBitMax || R.SignIsBitMax || D.SignIsBitMax) throw new Exception("分数的元素不能有负数");
            if (R>=D) throw new Exception("分子必须小于分母");

            Quotient = Q;
            Remainder = R;
            Denominator = D;

            Sign = BigDec.BitMin;
        }

        /// <summary>
        /// 转入
        /// </summary>
        public static implicit operator BigDecFen(string text)
        {
            if (text == "NaN") return NaN;
            if (string.IsNullOrEmpty(text)) return ZeroZeroOne;

            BigDecFen R = new BigDecFen();
            var mt = ExpressionEval.fenRegex.Match(text);
            if (mt != null)
            {
                foreach (Group mg in mt.Groups)
                {
                    Console.WriteLine("匹配 {0}({1}):{2}", "", mg.Index, mg.Value);
                }
            }
            return R;
        }


        /// <summary>
        /// 转字符串
        /// </summary>
        /// <returns></returns>
        public string ToString(long digits)
        {
            return this.ToDecimalString(digits);
        }

        /// <summary>
        /// 输出小数字符串
        /// </summary>
        /// <returns></returns>
        private string ToDecimalString(long digits)
        {
            if (!SignIsBitMax && !SignIsBitMin) return "NaN";//无效值
            if (Denominator.IsNaN || Quotient.IsNaN || Remainder.IsNaN) return "NaN";//无效值
            if (Denominator.IsZero && !Quotient.IsZero && !Remainder.IsZero) return "NaN";//无效值

            var sign = "";
            if (SignIsBitMax) sign = "-";

            var qstr = "";
            if (Quotient.IsZero) qstr = "0.";
            else qstr = Quotient.ToString() + ".";

            var fenstr = "";
            if (Remainder.IsZero) fenstr = "0";
            else fenstr = Div(Remainder, Denominator, digits);

            var bodystr = string.Format("{0}{1}{2}", sign, qstr, fenstr);

            return bodystr;
        }

        /// <summary>
        /// 除法
        /// </summary>
        /// <param name="remainder"></param>
        /// <param name="denominator"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        private string Div(BigDec remainder, BigDec denominator, long digits)
        {
            StringBuilder sbd = new StringBuilder();
            var R = remainder.Clone();
            for (var fi=0;fi<digits;fi++)
            {
                R = R << 1;//自动移位一次
                if (R < denominator) {
                    //Console.WriteLine("被除数：{0} / 除数：{1}",R, denominator);
                    sbd.Append(0);
                }
                else
                {
                    //取模
                    var fj = 0;
                    while(R >= denominator)
                    {
                        fj++;
                        R -= denominator;
                        //Console.WriteLine("结果：" + R);
                    }
                    sbd.Append(fj);
                    //Console.WriteLine("结束：" + fj);
                }
            };
            return sbd.ToString();
        }

        /// <summary>
        /// 转字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //Console.WriteLine("{0}+{1}/{2}", Denominator.IsNaN, Quotient.IsNaN, Remainder.IsNaN);
            if (!SignIsBitMax && !SignIsBitMin) return "NaN";//无效值
            if (Denominator.IsNaN || Quotient.IsNaN || Remainder.IsNaN) return "NaN";//无效值
            if (Denominator.IsZero && !Quotient.IsZero && !Remainder.IsZero) return "NaN";//无效值

            var sign="";
            if (SignIsBitMax) sign = "-";

            var qstr = "";
            if (Quotient.IsZero) qstr = "";
            else qstr = Quotient.ToString() + "+";

            var fenstr = "";
            if (Remainder.IsZero) fenstr = "";
            else fenstr = string.Format("{0}/{1}", Remainder, Denominator);

            var bodystr = string.Format("{0}{1}", qstr, fenstr);
            if (string.IsNullOrEmpty(bodystr))
            {
                bodystr = "0";
            }
            else
            {
                bodystr =string.Format("{0}({1})", sign, bodystr);
            }

            return bodystr;
        }

        #region "判断"
        /// <summary>
        /// 是否正数
        /// </summary>
        internal bool SignIsBitMin => Sign == BigDec.BitMin;
        /// <summary>
        /// 是否负数
        /// </summary>
        internal bool SignIsBitMax => Sign == BigDec.BitMax;
        #endregion


        #region "一元算术运算"
        /// <summary>
        /// 负号
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static BigDecFen operator +(BigDecFen A)
        {
            return A.Clone();
        }

        /// <summary>
        /// 负号
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static BigDecFen operator -(BigDecFen A){
            var R = A.Clone();
            R.Sign = BigDec.BitMax;
            return R;
            }

        /// <summary>
        /// 克隆一个副本 不影响源对象
        /// </summary>
        /// <returns></returns>
        private BigDecFen Clone()
        {
            return new BigDecFen()
            {
                Denominator = this.Denominator,
                Sign = this.Sign,
                Quotient = this.Quotient,
                Remainder = this.Remainder,
            };
        }
        #endregion

    }
}

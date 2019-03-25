using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.CompilerServices;
using System.Numerics;

namespace System.Numerics
{
    /// <summary>
    /// 大整数扩展
    /// </summary>
    public static class BigInteger_Ex
    {
        /// <summary>
        /// 是否10的次方
        /// </summary>
        public static bool IsPowerOfTen(this BigInteger value,ref int pow)
        {
            var valStr = value.ToString();
            var valEnd = valStr.TrimEnd('0');
            if (valEnd.TrimStart('0') == "1") {
                pow = valStr.Length - valEnd.Length;
                return true;
            }
            return false;
        }
    }


    /// <summary>
    /// 大值小数
    /// </summary>
    [Author("Linyee", "2019-02-24")]
    public struct BigDecimal:IComparable<BigDecimal>
    {
        #region "字段"
        /// <summary>
        /// 值
        /// </summary>
        private readonly BigInteger @value;

        /// <summary>
        /// E指数
        /// *10^TenPower
        /// 小数位置
        /// 正数往右 负数往左
        /// </summary>
        public readonly int TenPower;

        /// <summary>
        /// 符号值
        /// </summary>
        private int Sign;

        /// <summary>
        /// 最小精度
        /// 主要用于除法时的最小精度
        /// </summary>
        private int Precision;

        /// <summary>
        /// 设置精度
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public BigDecimal SetPrecision(int p)
        {
            this.Precision = p;
            return this;
        }

        /// <summary>
        /// 强制精度
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public BigDecimal MandPrecision(int p)
        {
            var val = this.Clone();
            if (-val.TenPower < p) val.DownTenPower(-p);
            var str = val.ToString();
            return this;
        }

        /// <summary>
        /// 设置精度
        /// </summary>
        /// <returns></returns>
        public int GetPrecision()
        {
            return this.Precision;
        }
        #endregion

        #region "构造"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public BigDecimal(float value)
        {
            if (value == 0) Sign = 0;
            else if (value < 0) Sign = -1;
            else if (value > 0) Sign = +1;
            this = BigDecimal.Parse(value.ToString("R"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public BigDecimal(double value)
        {
            if (value == 0) Sign = 0;
            else if (value < 0) Sign = -1;
            else if (value > 0) Sign = +1;
            this = BigDecimal.Parse(value.ToString("R"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public BigDecimal(decimal value):this()
        {
            if (value == 0) Sign = 0;
            else if (value < 0) Sign = -1;
            else if (value > 0) Sign = +1;
            this = BigDecimal.Parse(value.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public BigDecimal(long value) : this()
        {
            if (value == 0) Sign = 0;
            else if (value < 0) Sign = -1;
            else if (value > 0) Sign = +1;

            this.value = value;
            this.Precision = 36;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public BigDecimal(ulong value) : this()
        {
            if (value == 0) Sign = 0;
            else if (value > 0) Sign = +1;

            this.value = value;
            this.Precision = 36;
        }

        #region "科学计数"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public BigDecimal(BigInteger value) : this(value, 0)
        {
        }

        /// <summary>
        /// 科学计数 指数 
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="tenpower">*10^tenpower</param>
        public BigDecimal(BigInteger value, int tenpower) : this()
        {
            if (value == 0)
            {
                this.Sign = 0;
            }
            else if (value > 0)
            {
                this.@value = value;
                this.Sign = +1;
                this.TenPower = tenpower;
            }
            else if (value < 0)
            {
                this.@value = -value;
                this.Sign = -1;
                this.TenPower = tenpower;
            }

            if (-tenpower > 36) this.Precision = -tenpower;
            else this.Precision = 36;
            //Console.WriteLine("从BigInteger创建 符号{0}", Sign);
        }

        #endregion

        #region "同值调整"

        /// <summary>
        /// 同值调整 
        /// 克隆后调整E指数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tenpower"></param>
        public BigDecimal(BigDecimal value,int tenpower):this(value)
        {
            //相同
            if (tenpower == this.TenPower)
            {
                return ;
            }
            else if (tenpower < this.TenPower)
            {
                this.value *= BigInteger.Pow(10, this.TenPower- tenpower);
                this.TenPower = tenpower;

                if (-this.TenPower > this.Precision) this.Precision = -this.TenPower;
            }
            else
            {
                throw new InvalidOperationException("无法调高指数，请通过转字符串后再解析，实现最优指数!");
            }
        }



        /// <summary>
        /// 小数一致性
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static int SameTenPower(ref BigDecimal left, ref BigDecimal right)
        {
            int TenPower = Math.Min(left.TenPower, right.TenPower);
            left = left.DownTenPower(TenPower);
            right = right.DownTenPower(TenPower);
            return TenPower;
        }

        /// <summary>
        /// 降低指数
        /// </summary>
        /// <param name="tenPower"></param>
        /// <returns></returns>
        private BigDecimal DownTenPower(int tenPower)
        {
            if (tenPower == this.TenPower)
            {
                return this.Clone();
            }
            return new BigDecimal(this, tenPower);
        }


        /// <summary>
        /// 相当于Clone
        /// </summary>
        /// <param name="value"></param>
        public BigDecimal(BigDecimal value) :this()
        {
            this.Sign = value.Sign;
            this.value = value.value;
            this.TenPower = value.TenPower;
            this.Precision = value.Precision;
        }

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public BigDecimal Clone()
        {
            return new BigDecimal(this);
        }
        #endregion
        #endregion

        #region "常量"
        /// <summary>
        /// 0
        /// </summary>
        public static readonly BigDecimal Zero = new BigDecimal(0);

        /// <summary>
        /// 1
        /// </summary>
        public static readonly BigDecimal One = new BigDecimal(1);
        #endregion

        #region "判断"
        /// <summary>
        /// 是否0
        /// </summary>
        public bool IsZero
        {
            get
            {
                return this.value.IsZero;
            }
        }
        /// <summary>
        /// 是否1 内部值
        /// </summary>
        public bool IsOneValue
        {
            get
            {
                return this.value.IsOne;
            }
        }
        /// <summary>
        /// 是否1
        /// </summary>
        public bool IsOne
        {
            get
            {
                int pow = 0;
                if (this.TenPower == 0 && this.value.IsOne)
                {
                    return true;
                }
                else if (this.TenPower > 0 && this.value.IsPowerOfTen(ref pow) && this.TenPower == pow)
                {
                    //this.value = 1;
                    //this.TenPower = 0;
                    return true;
                }

                return false;
            }
        }
        #endregion

        #region "属性"
        #endregion

        #region "类型转入"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigDecimal(sbyte value)
        {
            return new BigDecimal((long)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigDecimal(byte value)
        {
            return new BigDecimal((long)((ulong)value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigDecimal(short value)
        {
            return new BigDecimal((long)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigDecimal(ushort value)
        {
            return new BigDecimal((long)((ulong)value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigDecimal(int value)
        {
            return new BigDecimal((long)value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigDecimal(uint value)
        {
            return new BigDecimal((long)((ulong)value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigDecimal(long value)
        {
            return new BigDecimal(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigDecimal(ulong value)
        {
            return new BigDecimal(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigDecimal(float value)
        {
            return new BigDecimal(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigDecimal(double value)
        {
            return new BigDecimal(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigDecimal(decimal value)
        {
            return new BigDecimal(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator BigDecimal(BigInteger value)
        {
            return new BigDecimal(value);
        }
        #endregion

        #region "比较运算"

        /// <summary>
        /// 比较器
        /// 1大于 0等于 小于
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(BigDecimal other)
        {
            if (this.Sign > other.Sign) return 1;//根据符号判断
            if (this.Sign < other.Sign) return -1;//根据符号判断

            if (this.Sign == other.Sign)
            {
                if(this.Sign ==0)//0
                {
                    return 0;
                }
                else if (this.Sign >0)//正数
                {
                    var tenpowrer =SameTenPower(ref this, ref other);
                    if (this.value > other.value) return +1;
                    if (this.value == other.value) return 0;
                    if (this.value < other.value) return -1;
                }
                else//负数
                {
                    var tenpowrer = SameTenPower(ref this, ref other);
                    if (this.value > other.value) return -1;
                    if (this.value == other.value) return 0;
                    if (this.value < other.value) return +1;
                }
            }
            throw new Exception("CompareTo Error");
        }

        /// <summary>
        /// 是否相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            BigDecimal tval = Zero;
            try
            {
                tval = From(obj);
                return this == tval;
            }
            catch
            {
                return false;
            }
            //return base.Equals(obj);
        }

        /// <summary>
        /// 哈希值
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// 大于
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(BigDecimal left, BigDecimal right) {
            var comp = left.CompareTo(right);
            return comp == 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <(BigDecimal left, BigDecimal right)
        {
            var comp = left.CompareTo(right);
            return comp == -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >=(BigDecimal left, BigDecimal right)
        {
            var comp = left.CompareTo(right);
            return comp >=0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <=(BigDecimal left, BigDecimal right)
        {
            var comp = left.CompareTo(right);
            return comp <= 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(BigDecimal left, BigDecimal right)
        {
            var comp = left.CompareTo(right);
            return comp == 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(BigDecimal left, BigDecimal right)
        {
            var comp = left.CompareTo(right);
            return comp != 0;
        }
        #endregion

        #region "一元运算"
        /// <summary>
        /// 正号
        /// </summary>
        /// <param name="left"></param>
        /// <returns></returns>
        public static BigDecimal operator +(BigDecimal left)
        {
            if (left.IsZero) return BigDecimal.Zero;
            return left.Clone();
        }
        /// <summary>
        /// 负号
        /// </summary>
        /// <param name="left"></param>
        /// <returns></returns>
        public static BigDecimal operator -(BigDecimal left)
        {
            if (left.IsZero) return BigDecimal.Zero;
            var val= left.Clone();
            val.Sign = -val.Sign;
            return val;
        }
        /// <summary>
        /// 正号
        /// </summary>
        /// <param name="left"></param>
        /// <returns></returns>
        public static BigDecimal operator ++(BigDecimal left)
        {
            return new BigDecimal(left.value+1);
        }
        /// <summary>
        /// 负号
        /// </summary>
        /// <param name="left"></param>
        /// <returns></returns>
        public static BigDecimal operator --(BigDecimal left)
        {
            return new BigDecimal(left.value - 1);
        }
        #endregion

        #region "四则运算"

        /// <summary>
        /// 加法
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static BigDecimal operator +(BigDecimal left, BigDecimal right)
        {
            if (right.IsZero) return new BigDecimal(left);
            int tempower = SameTenPower(ref left, ref right);
            var RR= new BigDecimal(left.Sign*left.@value + right.Sign * right.@value, tempower);
            return RR; 
        }

        /// <summary>
        /// 减法
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static BigDecimal operator -(BigDecimal left, BigDecimal right)
        {
            if (right.IsZero) return new BigDecimal(left);
            int tempower = SameTenPower(ref left, ref right);
            var RR = new BigDecimal(left.Sign * left.@value - right.Sign * right.@value, tempower);
            return RR;
        }

        /// <summary>
        /// 乘法
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static BigDecimal operator *(BigDecimal left, BigDecimal right)
        {
            if (left.IsZero || right.IsZero) return BigDecimal.Zero;
            if (left.IsOne) return new BigDecimal(right);
            if (right.IsOne) return new BigDecimal(left);

            BigInteger value = left.@value * right.@value;
            int tempower = left.TenPower + right.TenPower;
            if (tempower > int.MaxValue)
            {
                throw new Exception("大值指数溢出");
            }

            var RR= new BigDecimal(value, tempower);
            if (left.Sign == right.Sign) RR.Sign = 1;
            else RR.Sign = -1;
            return RR;
        }

        /// <summary>
        /// 除法
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static BigDecimal operator /(BigDecimal left, BigDecimal right)
        {
            //Console.WriteLine("=进入=大值小数除数");

            if (left.IsZero) return BigDecimal.Zero;
            if (right.IsZero) throw new Exception("除零溢出");
            if (right.IsOne) return new BigDecimal(left);

            var P = SamePrecision(ref left,ref right);//最大精度
            var T = SameTenPower(ref left,ref right);//最小指数
            var A = new BigDecimal(left,T- P);//提升精度;
            BigInteger value = A.@value / right.@value;
            var R = new BigDecimal(value,-P);

            if (left.Sign == right.Sign)
            {
                return R;
            }
            return -R;
        }

        /// <summary>
        /// 取模
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static BigDecimal operator %(BigDecimal left, BigDecimal right)
        {
            //原数为0，返回0
            if (left.IsZero) return BigDecimal.Zero;
            //模为0，返回原数
            if (right.IsZero) return left.Clone();
            //模为1，返回小数部分
            if (right.IsOne)
            {
                if(left.TenPower>=0)return BigDecimal.Zero;

                string result = left.ToString();
                var str = result.Split('.')[1];
                return Parse("0." + str);
            }

            BigDecimal B = right.Clone();
            if (B.TenPower > 0) B = new BigDecimal(B, 0);
            var T = SameTenPower(ref left,ref B);//一致小数性

            BigInteger value = left.@value % B.@value;
            var R = new BigDecimal(value, T);
            R.Sign = left.Sign;
            return R;
        }

        /// <summary>
        /// 同步精度
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static int SamePrecision(ref BigDecimal left, ref BigDecimal right)
        {
            var max = Math.Max(left.Precision, right.Precision);
            left.Precision = max;
            right.Precision = max;
            return max;
        }
        #endregion

        #region "高级运算"
        /// <summary>
        /// 指数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pow"></param>
        /// <returns></returns>
        public static BigDecimal Pow(BigDecimal value, int pow)
        {
            var A = value.value;
            var R = BigInteger.Pow(A,pow);
            var T = value.TenPower * pow;
            return new BigDecimal(R,T);
        }

        /// <summary>
        /// 最小值
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static BigDecimal Min(BigDecimal a, BigDecimal b)
        {
            var T = SameTenPower(ref a, ref b);
            if (a.value < b.value) return a;
            return b;
        }

        /// <summary>
        /// 最大值
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static BigDecimal Max(BigDecimal a, BigDecimal b)
        {
            var T = SameTenPower(ref a, ref b);
            if (a.value > b.value) return a;
            return b;
        }
        #endregion

        #region "转字符串"
        /// <summary>
        /// 转字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //Console.WriteLine("value={0} sign={1} tenpower={2} p={3}",this.value.ToString(),this.Sign,this.TenPower,this.Precision);
            var fh= Sign < 0 ? "-" : "";
            if (this.TenPower == 0)
            {
                return fh+this.@value.ToString();
                //return string.Concat(this.@value.ToString(), ".");
            }
            else if(this.TenPower > 0)
            {
                return string.Concat(Sign < 0 ? "-" : "", this.@value.ToString(), new string('0',this.TenPower));
            }
            else
            {
                string result = this.@value.ToString();
                var scale = -this.TenPower;
                if (result.Length > scale)
                {
                    return fh+ result.Insert(result.Length - scale, ".");
                }
                return string.Concat(fh, "0.", new string('0', scale - result.Length), result);
            }
        }
        /// <summary>
        /// 转字符串
        /// </summary>
        /// <returns></returns>
        public string ToString(string fmt)
        {
            if (string.IsNullOrEmpty(fmt)) return ToString();
            var ftype = fmt[0];
            var str = ToString();
            switch (ftype)
            {
                case 'F':
                case 'f':
                    {
                        //输出精度
                        var p = this.Precision;
                        if (fmt.Length > 1) p = int.Parse(fmt.Substring(1));

                        //添加尾0
                        var pindex = str.IndexOf(".");
                        if (pindex < 0) {
                            str = string.Concat(str, ".");
                            pindex = str.Length - 1;
                            str = string.Concat(str, new string('0',p));
                        }

                        //当前精度
                        int cp = str.Length - pindex;
                        if(cp<p) str = string.Concat(str, new string('0', p-cp));
                    }
                    break;
                default:
                    break;
            }

            //输出格式化过的数据
            return str;
        }
        #endregion

        #region　"解析"

        /// <summary>
        /// 从对象创建
        /// </summary>
        /// <param name="value"></param>
        public static BigDecimal From(object value)
        {
            var tc = Convert.GetTypeCode(value);
            switch (tc)
            {
                case TypeCode.Boolean:
                    return new BigDecimal(((bool)value) ? 1 : 0);
                case TypeCode.DateTime:
                    return new BigDecimal(((DateTime)value).Ticks);
                case TypeCode.DBNull:
                case TypeCode.Empty:
                    throw new Exception("不支持null的转换");
                case TypeCode.SByte:
                    return new BigDecimal((long)(SByte)value);
                case TypeCode.Int16:
                    return new BigDecimal((long)(Int16)value);
                case TypeCode.Int32:
                    return new BigDecimal((long)(Int32)value);
                case TypeCode.Int64:
                    return new BigDecimal((long)(Int64)value);
                case TypeCode.Char:
                    return Parse("" + value);
                case TypeCode.Byte:
                    return new BigDecimal((ulong)value);
                case TypeCode.UInt16:
                    return new BigDecimal((ulong)(UInt16)value);
                case TypeCode.UInt32:
                    return new BigDecimal((ulong)(UInt32)value);
                case TypeCode.UInt64:
                    return new BigDecimal((ulong)(UInt64)value);
                case TypeCode.Single:
                    return new BigDecimal((Single)value);
                case TypeCode.Double:
                    return new BigDecimal((Double)value);
                case TypeCode.Decimal:
                    return new BigDecimal((Decimal)value);
                case TypeCode.String:
                    return Parse(value?.ToString());
                case TypeCode.Object:
                    if (typeof(BigInteger) == value.GetType()) return new BigDecimal((BigInteger)value);
                    if (typeof(BigDecimal) == value.GetType()) return ((BigDecimal)value).Clone();
                    return Parse(value?.ToString());
                default:
                    throw new Exception("不支持的转换");
            }
        }

        /// <summary>
        /// 解析字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static BigDecimal Parse(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str", "BigDecimal.Parse: Cannot parse null");
            }
            //e指数 *十的冥指数
            int tenpower = 0;
            //值
            StringBuilder valueBuilder = new StringBuilder();
            //指数
            StringBuilder stringBuilder = null;

            BigDecimal.ParseState parseState = BigDecimal.ParseState.Start;
            //错误
            Action<char> formatException = (char c) => throw new FormatException(string.Concat(new object[] { "BigDecimal.Parse: invalid character '", c, "' in: ", str }));
            Action startExponent = () => {
                stringBuilder = new StringBuilder();
                parseState = BigDecimal.ParseState.E;
            };
            string str1 = str;

            for (int i = 0; i < str1.Length; i++)
            {
                char chr = str1[i];
                switch (parseState)
                {
                    case BigDecimal.ParseState.Start:
                        {
                            //[0-9\-\+] 
                            if (char.IsDigit(chr) || chr == '-' || chr == '+')
                            {
                                parseState = BigDecimal.ParseState.Integer;
                                valueBuilder.Append(chr);
                                break;
                            }
                            else if (chr != '.')
                            {
                                formatException(chr);
                                break;
                            }
                            else
                            {
                                parseState = BigDecimal.ParseState.Decimal;
                                break;
                            }
                        }
                    case BigDecimal.ParseState.Integer:
                        {
                            if (char.IsDigit(chr))
                            {
                                valueBuilder.Append(chr);
                                break;
                            }
                            else if (chr == '.')
                            {
                                parseState = BigDecimal.ParseState.Decimal;
                                break;
                            }
                            else if (chr == 'e' || chr == 'E')
                            {
                                startExponent();
                                break;
                            }
                            else
                            {
                                formatException(chr);
                                break;
                            }
                        }
                    case BigDecimal.ParseState.Decimal:
                        {
                            if (char.IsDigit(chr))
                            {
                                tenpower = checked(checked(tenpower - 1));
                                valueBuilder.Append(chr);
                                if (tenpower < 0) str1 = str1.TrimEnd('0');
                                break;
                            }
                            else if (chr == 'e' || chr == 'E')
                            {
                                startExponent();
                                break;
                            }
                            else
                            {
                                formatException(chr);
                                break;
                            }
                        }
                    case BigDecimal.ParseState.E:
                        {
                            if (char.IsDigit(chr) || chr == '-' || chr == '+')
                            {
                                parseState = BigDecimal.ParseState.Exponent;
                                stringBuilder.Append(chr);
                                break;
                            }
                            else
                            {
                                formatException(chr);
                                break;
                            }
                        }
                    case BigDecimal.ParseState.Exponent:
                        {
                            if (!char.IsDigit(chr))
                            {
                                formatException(chr);
                                break;
                            }
                            else
                            {
                                stringBuilder.Append(chr);
                                break;
                            }
                        }
                }
            }

            //不是数值
            if (valueBuilder.Length == 0 || valueBuilder.Length == 1 && !char.IsDigit(valueBuilder[0]))
            {
                throw new FormatException(string.Concat("BigDecimal.Parse: string didn't contain a value: \"", str, "\""));
            }
            //指数不是数值
            if (stringBuilder != null && (stringBuilder.Length == 0 || valueBuilder.Length == 1 && !char.IsDigit(valueBuilder[0])))
            {
                throw new FormatException(string.Concat("BigDecimal.Parse: string contained an 'E' but no exponent value: \"", str, "\""));
            }

            //数值 允许负值
            BigInteger value = BigInteger.Parse(valueBuilder.ToString());

            //E指数n a*10^n
            if (stringBuilder != null)
            {
                int exponent = int.Parse(stringBuilder.ToString());
                tenpower += exponent;
            }
            return new BigDecimal(value, tenpower);
        }

        /// <summary>
        /// 解析状态
        /// </summary>
        private enum ParseState
        {
            /// <summary>
            /// 开始
            /// </summary>
            Start,
            /// <summary>
            /// 整数
            /// </summary>
            Integer,
            /// <summary>
            /// 小数
            /// </summary>
            Decimal,
            /// <summary>
            /// 指数开始
            /// </summary>
            E,
            /// <summary>
            /// 指数
            /// </summary>
            Exponent
        }
        #endregion
    }
}

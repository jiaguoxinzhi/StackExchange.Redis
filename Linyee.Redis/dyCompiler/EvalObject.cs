using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using WS_Server.dyCompiler;

namespace WS_Core.dyCompiler
{


    /// <summary>
    /// 值对象
    /// </summary>
    [Author("Linyee", "2019-02-03")]
    public sealed class EvalObject<T>: EvalObjectBase
    {
        /// <summary>
        /// 表示一个空值
        /// </summary>
        internal static readonly EvalObject NaN = new EvalObject()
        {
            Type = EvalObjectType.Empty,
            Value = null,
        };

        /// <summary>
        /// 
        /// </summary>
        internal EvalObject() { }

        /// <summary>
        /// 
        /// </summary>
        public EvalObject(object obj) : base(obj) {
            Value = (T)base.Value;
        }

        #region "字段"
        /// <summary>
        /// 值
        /// </summary>
        public new T Value = default(T);
        #endregion

        #region "隐式转换"
        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(EvalObject value)
        {
            var tc = Convert.GetTypeCode(default(T)).ToString();

            //大小数
            if (typeof(T) == typeof(BigDecimal))
            {
                tc = "BigDecimal";
            }

            //大小数
            if (typeof(T) == typeof(BigDecimal))
            {
                tc = "BigDecimal";
            }

            //值类型转换
            if (Enum.IsDefined(typeof(EvalObjectType), tc))
            {
                var type = (EvalObjectType)Enum.Parse(typeof(EvalObjectType), tc);
                if(type== value.Type)
                {
                    return new EvalObject<T>()
                    {
                        Type = value.Type,
                        Value = (T)value.Value,
                    };
                }
                else if (type ==  EvalObjectType.UInt64 && value.Type== EvalObjectType.Int64)
                {
                    return new EvalObject<T>()
                    {
                        Type = EvalObjectType.UInt64,
                        Value = (T)(object)(ulong)(long)value.Value,
                    };
                }
                else if (type == EvalObjectType.Decimal && (int)value.Type <(int)EvalObjectType.Decimal)
                {
                    return new EvalObject<T>()
                    {
                        Type = EvalObjectType.Decimal,
                        Value = (T)(object)decimal.Parse(value.Value.ToString()),
                    };
                }
                else if (type == EvalObjectType.Double && (int)value.Type < (int)EvalObjectType.Double)
                {
                    return new EvalObject<T>()
                    {
                        Type = EvalObjectType.Double,
                        Value = (T)(object)double.Parse(value.Value.ToString()),
                    };
                }
                else if (type == EvalObjectType.BigInteger && (int)value.Type < (int)EvalObjectType.BigInteger)
                {
                    return new EvalObject<T>()
                    {
                        Type = EvalObjectType.BigInteger,
                        Value = (T)(object)BigInteger.Parse(value.Value.ToString()),
                    };
                }
                else if (type == EvalObjectType.BigDecimal && (int)value.Type < (int)EvalObjectType.BigDecimal)
                {
                    return new EvalObject<T>()
                    {
                        Type = EvalObjectType.BigDecimal,
                        Value = (T)(object)BigDecimal.Parse(value.Value.ToString()),
                    };
                }
                else
                {
                    throw new Exception(string.Format( "无法隐式转换，请使用强制转换{0} {1}", type,tc));
                }
            }
            else
            {
                throw new Exception("泛型T必须是限定的类型"+string.Join(",", Enum.GetNames(typeof(EvalObjectType))));
            }
        }
        #endregion

        #region "隐式转换"

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(string value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(bool value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(DateTime value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(Single value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(double value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(decimal value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(byte value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(sbyte value)
        {
            return value.eval();
        }


        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(Int16 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(Int32 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(Int64 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(UInt16 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(UInt32 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(UInt64 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(BigInteger value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject<T>(BigDecimal value)
        {
            return value.eval();
        }
        #endregion
    }


    /// <summary>
    /// 值对象
    /// </summary>
    [Author("Linyee", "2019-02-03")]
    public sealed class EvalObject: EvalObjectBase
    {
        /// <summary>
        /// 表示一个空值
        /// </summary>
        internal static readonly EvalObject NaN = new EvalObject()
        {
            Type = EvalObjectType.Empty,
            Value = null,
        };

        /// <summary>
        /// 
        /// </summary>
        internal EvalObject()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public EvalObject(object obj) : base(obj) { }

        /// <summary>
        /// 克隆一个副本
        /// </summary>
        /// <returns></returns>
        internal EvalObject Clone()
        {
            return new EvalObject()
            {
                Type = this.Type,
                Value = this.Value,
            };
        }


        #region "隐式转换"

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(string value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(bool value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(DateTime value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(Single value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(double value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(decimal value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(byte value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(sbyte value)
        {
            return value.eval();
        }


        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(Int16 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(Int32 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(Int64 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(UInt16 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(UInt32 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(UInt64 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(BigInteger value)
        {
            return value.eval();
        }
        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObject(BigDecimal value)
        {
            return value.eval();
        }

        #endregion

        #region "运算符"
        /// <summary>
        /// 
        /// </summary>
        public static EvalObject operator *(EvalObject A, EvalObject B)
        {
            if(A.Type== EvalObjectType.BigInteger && B.Type== EvalObjectType.BigInteger)
            {
                var AA= (BigInteger)A.Value;
                var BB = (BigInteger)B.Value;
                var RR= AA * BB;
                //Console.WriteLine("整数 {3}=>{0}*{1}={2}", AA, BB, RR,A.Value);
                return RR;
            }
            else if ((A.Type == EvalObjectType.BigInteger || A.Type == EvalObjectType.BigDecimal || A.Type == EvalObjectType.Double || A.Type == EvalObjectType.Decimal)
                && (B.Type == EvalObjectType.BigInteger || B.Type == EvalObjectType.BigDecimal || B.Type == EvalObjectType.Double || B.Type == EvalObjectType.Decimal))
            {
                BigDecimal AA = BigDecimal.From(A.Value);
                BigDecimal BB = BigDecimal.From(B.Value);
                var RR = AA * BB;
                //Console.WriteLine("小数 {3}=>{0}*{1}={2}", AA, BB, RR, A.Value);
                return RR;
            }
            else
            {
                throw new Exception(A.Type+"不支持此类型的直接*运算"+B.Type);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static EvalObject operator /(EvalObject A, EvalObject B)
        {
            if (A.Type == EvalObjectType.BigInteger && B.Type == EvalObjectType.BigInteger)
            {
                var AA = (BigInteger)A.Value;
                var BB = (BigInteger)B.Value;
                var RR = AA * BB;
                //Console.WriteLine("整数 {3}=>{0}*{1}={2}", AA, BB, RR,A.Value);
                return RR;
            }
            else if ((A.Type == EvalObjectType.BigInteger || A.Type == EvalObjectType.BigDecimal || A.Type == EvalObjectType.Double || A.Type == EvalObjectType.Decimal)
                && (B.Type == EvalObjectType.BigInteger || B.Type == EvalObjectType.BigDecimal || B.Type == EvalObjectType.Double || B.Type == EvalObjectType.Decimal))
            {
                BigDecimal AA = BigDecimal.From(A.Value);
                BigDecimal BB = BigDecimal.From(B.Value);
                var RR= AA / BB;
                //Console.WriteLine("{0}/{1}={2}", AA, BB, RR);
                return new EvalObject(RR);
            }
            else
            {
                throw new Exception(A.Type + "不支持此类型的直接/运算" + B.Type);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static EvalObject operator +(EvalObject A, EvalObject B)
        {
            if (A.Type == EvalObjectType.BigInteger && B.Type == EvalObjectType.BigInteger)
            {
                var AA = (BigInteger)A.Value;
                var BB = (BigInteger)B.Value;
                var RR = AA + BB;
                //Console.WriteLine("整数 {3}=>{0}+{1}={2}", AA, BB, RR,A.Value);
                return RR;
            }
            else if ((A.Type == EvalObjectType.BigInteger || A.Type == EvalObjectType.BigDecimal || A.Type == EvalObjectType.Double || A.Type == EvalObjectType.Decimal)
                && (B.Type == EvalObjectType.BigInteger || B.Type == EvalObjectType.BigDecimal || B.Type == EvalObjectType.Double || B.Type == EvalObjectType.Decimal))
            {
                BigDecimal AA = BigDecimal.From(A.Value);
                BigDecimal BB = BigDecimal.From(B.Value);
                var RR = AA + BB;
                //Console.WriteLine("小数 {3}=>{0}+{1}={2}", AA, BB, RR, A.Value);
                return RR;
            }
            else
            {
                throw new Exception(A.Type + "不支持此类型的直接+运算" + B.Type);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static EvalObject operator -(EvalObject A, EvalObject B)
        {
            if (A.Type == EvalObjectType.BigInteger && B.Type == EvalObjectType.BigInteger)
            {
                return ((BigInteger)A.Value) - ((BigInteger)B.Value);
            }
            else if ((A.Type == EvalObjectType.BigInteger || A.Type == EvalObjectType.BigDecimal || A.Type == EvalObjectType.Double || A.Type == EvalObjectType.Decimal ) 
                && (B.Type == EvalObjectType.BigInteger || B.Type == EvalObjectType.BigDecimal || B.Type == EvalObjectType.Double || B.Type == EvalObjectType.Decimal))
            {
                BigDecimal AA = BigDecimal.From(A.Value);
                BigDecimal BB = BigDecimal.From(B.Value);
                return AA - BB;
            }
            else
            {
                throw new Exception(A.Type + "不支持此类型的直接-运算" + B.Type);
            }
        }
        #endregion
    }
}

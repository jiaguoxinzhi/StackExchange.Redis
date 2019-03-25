using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using WS_Core.dyCompiler;

namespace WS_Server.dyCompiler
{

    /// <summary>
    /// 值对象
    /// </summary>
    [Author("Linyee", "2019-02-03")]
    public abstract class EvalObjectBase
    {

        /// <summary>
        /// 默认 long 0
        /// </summary>
        protected internal EvalObjectBase()
        {
        }

        /// <summary>
        /// 从对象创建
        /// </summary>
        /// <param name="value"></param>
        public EvalObjectBase(object value)
        {

            var tc = Convert.GetTypeCode(value);
            var valType = value.GetType();
            object result = 0L;
            //Console.WriteLine("类型："+tc.ToString()+","+ valType);
            switch (tc)
            {
                case TypeCode.Byte:
                    result = (long)(Byte)value;
                    break;
                case TypeCode.Boolean:
                    result = 1L;
                    break;
                case TypeCode.Char:
                    result = (long)(int)value;
                    break;
                case TypeCode.DateTime:
                    result = ((DateTime)value).Ticks;
                    break;
                case TypeCode.DBNull:
                    result = 0L;
                    break;
                case TypeCode.Decimal:
                    result = (decimal)value;
                    break;
                case TypeCode.Double:
                    result = (double)value;
                    break;
                case TypeCode.Empty:
                    result = 0L;
                    break;
                case TypeCode.Int16:
                    result = (long)(Int16)value;
                    break;
                case TypeCode.Int32:
                    result = (long)(Int32)value;
                    break;
                case TypeCode.Int64:
                    result = (long)value;
                    break;
                case TypeCode.Object:
                    {
                        EvalObject res = EvalObject.NaN;
                        if (valType == typeof(EvalObject))
                        {
                            res = (EvalObject)value;
                        }
                        else if (valType == typeof(BigInteger))
                        {
                            res = new EvalObject()
                            {
                                Type = EvalObjectType.BigInteger,
                                Value = (BigInteger)value,
                            };
                        }
                        else if (valType == typeof(BigDecimal))
                        {
                            res = new EvalObject()
                            {
                                Type = EvalObjectType.BigDecimal,
                                Value = (BigDecimal)value,
                            };
                        }
                        else
                        {
                            res = (value?.ToString()).eval();
                        }
                        Type = res.Type;
                        Value = res.Value;
                    }
                    return;
                case TypeCode.SByte:
                    result = (long)(SByte)value;
                    break;
                case TypeCode.Single:
                    result = (double)(Single)value;
                    break;
                case TypeCode.String:
                    {
                        var res = (value?.ToString()).eval();
                        Type = res.Type;
                        Value = res.Value;
                    }
                    return;
                case TypeCode.UInt16:
                    result = (long)(UInt16)value;
                    break;
                case TypeCode.UInt32:
                    result = (long)(UInt32)value;
                    break;
                case TypeCode.UInt64:
                    result = (ulong)value;
                    break;
                default:
                    throw new Exception("未知的类别");
            }

            Type = (EvalObjectType)Enum.Parse(typeof(EvalObjectType), Convert.GetTypeCode(result).ToString());
            Value = result;
        }

        #region "字段"
        /// <summary>
        /// 类型
        /// </summary>
        internal EvalObjectType Type = EvalObjectType.Int64;
        /// <summary>
        /// 值
        /// </summary>
        internal object Value = 0L;
        #endregion

        /// <summary>
        /// 输出值
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// 输出值
        /// </summary>
        /// <returns></returns>
        public string ToString(string fmt)
        {
            if (fmt == null) return ToString();

            var ch = fmt[0];
            int len = 36;
            if (fmt.Length > 1) len = int.Parse(fmt.Substring(1));
            switch (ch)
            {
                case 'f':
                case 'F':
                    return ToString("F", len);
            }

            return ToString();
        }

        /// <summary>
        /// 输出值
        /// </summary>
        /// <returns></returns>
        public string ToString(string fmt, int per)
        {
            var str = Value.ToString();
            int point = 0;
            var pi = str.IndexOf(".");
            if (pi < 0) {
                str += ".";
                pi = str.Length - 1;
            }
            if (pi >= 0) point = str.Length - pi;

            if (point < per)
            {
                str += new string('0', per - point);
            }
            else if(per< point)
            {
                str = str.Substring(0, pi + 1 + per);
            }
            return str;
        }

        #region "隐式转换"

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(string value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(bool value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(DateTime value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(Single value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(double value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(decimal value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(byte value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(sbyte value)
        {
            return value.eval();
        }


        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(Int16 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(Int32 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(Int64 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(UInt16 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(UInt32 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(UInt64 value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(BigInteger value)
        {
            return value.eval();
        }

        /// <summary>
        /// 
        /// </summary>
        public static implicit operator EvalObjectBase(BigDecimal value)
        {
            return value.eval();
        }
        #endregion
    }
}

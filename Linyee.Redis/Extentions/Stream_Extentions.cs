using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace System.IO
{
    /// <summary>
    /// 流扩展
    /// </summary>
    public static class Stream_Extentions
    {

        #region "无符"
        /// <summary>
        /// 读取 ulong
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static ulong ReadUInt64(this Stream stream, long offset = -1)
        {
            if(offset>=0) stream.Position = offset;
            var buf = new byte[8];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToUInt64(buf,0);
        }
        /// <summary>
        /// 读取 uint
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static uint ReadUInt(this Stream stream, long offset = -1)
        {
            if(offset>=0) stream.Position = offset;
            var buf = new byte[4];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToUInt32(buf,0);
        }
        /// <summary>
        /// 读取 Ushort
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static ushort ReadUshort(this Stream stream, long offset = -1)
        {
            if(offset>=0) stream.Position = offset;
            var buf = new byte[2];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToUInt16(buf,0);
        }
        /// <summary>
        /// 读取 byte
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static byte ReadByte(this Stream stream, long offset = -1)
        {
            if(offset>=0) stream.Position = offset;
            var buf = new byte[1];
            stream.Read(buf, 0, buf.Length);
            return buf[0];
        }
        #endregion

        #region "有符"
        /// <summary>
        /// 读取 sbyte
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static sbyte ReadSbyte(this Stream stream, long offset = -1)
        {
            return ReadInt8(stream,offset);
        }
        /// <summary>
        /// 读取 sbyte
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static sbyte ReadInt8(this Stream stream, long offset = -1)
        {
            if(offset>=0) stream.Position = offset;
            var buf = new byte[1];
            stream.Read(buf, 0, buf.Length);
            return (sbyte)buf[0];
        }
        /// <summary>
        /// 读取 short
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static short ReadInt16(this Stream stream, long offset = -1)
        {
            if(offset>=0) stream.Position = offset;
            var buf = new byte[2];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToInt16(buf,0);
        }
        /// <summary>
        /// 读取 int
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int ReadInt(this Stream stream, long offset = -1)
        {
            return ReadInt32(stream,offset);
        }
        /// <summary>
        /// 读取 int
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int ReadInt32(this Stream stream, long offset = -1)
        {
            if(offset>=0) stream.Position = offset;
            var buf = new byte[4];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToInt32(buf,0);
        }
        /// <summary>
        /// 读取 long
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static long ReadInt64(this Stream stream, long offset = -1)
        {
            if(offset>=0) stream.Position = offset;
            var buf = new byte[8];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToInt64(buf,0);
        }

        /// <summary>
        /// 读取 long
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static long ReadLong(this Stream stream, long offset = -1)
        {
            return ReadInt64(stream,offset);
        }

        #endregion
        #region "浮点"

        /// <summary>
        /// 读取 long
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Decimal ReadDecimal(this Stream stream, long offset = -1)
        {
            BinaryReader br=new BinaryReader(stream);
            {
                return br.ReadDecimal();
            }
        }
        /// <summary>
        /// 读取 Single
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Single ReadSingle(this Stream stream, long offset = -1)
        {
            if(offset>=0) stream.Position = offset;
            var buf = new byte[4];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToSingle(buf,0);
        }
        /// <summary>
        /// 读取 Double
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Double ReadDouble(this Stream stream, long offset = -1)
        {
            if(offset>=0) stream.Position = offset;
            var buf = new byte[8];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToDouble(buf,0);
        }
        #endregion

        #region 字符串
        /// <summary>
        /// 读取一行
        /// 无长度头
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public static string ReadLine2(this BinaryReader br)
        {
            StringBuilder sbd = new StringBuilder();
            char c = '\x20';
            while (true)
            {
                //已经结束
                if (br.BaseStream.Position < 0 || br.BaseStream.Position >= br.BaseStream.Length)
                {
                    if (sbd.Length > 0) return sbd.ToString();
                    else return "";
                }

                c = (char)br.ReadByte();
                if (c == '\r' || c == '\n')
                {
                    break;
                }
                else
                {
                    sbd.Append(c);
                }
            }


            skipSpace(br);
            //if (c == '\r' && br.PeekChar() == 10)
            //{
            //    br.ReadChar();
            //}

            return sbd.ToString();
        }

        /// <summary>
        /// 跳过空白
        /// </summary>
        /// <param name="br"></param>
        private static void skipSpace(this BinaryReader br)
        {
            var ch = ReadByteUntilNoSpace(br);
        }

        /// <summary>
        /// 读取一个非空字符
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public static char ReadByteUntilNoSpace(this BinaryReader br)
        {
            var ch= '\r';
            char[] spaces = new char[] { '\x00','\r','\n','\x7f'};
            while (spaces.Contains((char)br.PeekChar()))
            {
                ch = br.ReadChar();
                if (br.BaseStream.Position < 0 || br.BaseStream.Position >= br.BaseStream.Length) break;
            }
            return ch;
        }

        /// <summary>
        /// 读取一行
        /// 无长度头
        /// </summary>
        /// <param name="br"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string ReadLine2(this BinaryReader br, long count)
        {
            return ReadLine2(br, (int)count);
        }

        /// <summary>
        /// 读取一行
        /// 无长度头
        /// </summary>
        /// <param name="br"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string ReadLine2(this BinaryReader br,int count)
        {
            StringBuilder sbd = new StringBuilder();
            var buf = br.ReadBytes(count);
            skipSpace(br);
            foreach (var bt in buf){
                sbd.Append((char)bt);
            }
            return sbd.ToString();
        }

        #region "读取"
        /// <summary>
        /// 读取一行
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public static string ReadLine(this BinaryReader br)
        {
            var len = br.ReadByte();
            var buf =br.ReadBytes(len);
            if((buf[buf.Length - 2] == 13 || buf[buf.Length - 2] == 10) && buf[buf.Length - 1] == 10)
            {
                buf = buf.Take(buf.Length - 2).ToArray();
            }else if ((buf[buf.Length - 1] == 13 || buf[buf.Length - 1] == 10))
            {
                buf = buf.Take(buf.Length - 1).ToArray();
            }
            return Encoding.UTF8.GetString(buf);
        }
        #endregion

        #region "写入指定类型"
        /// <summary>
        /// 写入一行
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static BinaryWriter WriteLine(this BinaryWriter bw,string text)
        {
            StringBuilder sbd = new StringBuilder(text);
            sbd.AppendLine();
            bw.Write(sbd.ToString());
            return bw;
        }
        /// <summary>
        /// 写入一行
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BinaryWriter WriteLong(this BinaryWriter bw,long value)
        {
            bw.Write(value);
            return bw;
        }
        /// <summary>
        /// 写入一行
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BinaryWriter WriteByte(this BinaryWriter bw,byte value)
        {
            bw.Write(value);
            return bw;
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ReadString(this Stream stream, int length, Encoding encoding=null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
             var buf = new byte[length];
            stream.Read(buf, 0, length);
            return encoding.GetString(buf);
        }
        #endregion

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="stringType"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ReadString(this Stream stream, byte stringType=1, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var lenbuf = new byte[4];
            stream.Read(lenbuf, 0, stringType);
            var length = BitConverter.ToInt32(lenbuf,0);

            var buf = new byte[length];
            stream.Read(buf, 0, length);
            return encoding.GetString(buf);
        }
        #endregion

        #region 字节 切片
        /// <summary>
        /// 读取切片数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Span<byte> ReadSpanBytes(this Stream stream, long offset = -1)
        {
            return stream.ReadBytes(offset);
        }
        /// <summary>
        /// 读取字节数
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static byte[] ReadBytes(this Stream stream, long offset = -1)
        {
            if (offset >= 0) stream.Position = offset;
            var buf = new byte[stream.Length - offset];
            stream.Read(buf, 0, buf.Length);
            return buf;
        }
        #endregion
    }
}

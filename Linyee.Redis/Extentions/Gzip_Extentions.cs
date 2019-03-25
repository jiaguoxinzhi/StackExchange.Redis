using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace System.Text
{
    /// <summary>
    /// Gzip
    /// </summary>
    [Author("Linyee", "2019-02-01")]
    public static class Gzip_Extentions
    {
        #region "flater 初步测试比zip好用"
        /// <summary>
        /// 压缩算法
        /// </summary>
        /// <param name="rawString"></param>
        /// <returns></returns>
        public static string FlaterCompressString(this string rawString)
        {
            if (string.IsNullOrEmpty(rawString) || rawString.Length == 0) return "";

            var buf = Encoding.UTF8.GetBytes(rawString);
            var resbuf = FlaterCompress(buf);
            return Convert.ToBase64String(resbuf);
        }

        /// <summary>
        /// 压缩算法
        /// </summary>
        /// <param name="pBytes"></param>
        /// <returns></returns>
        public static byte[] FlaterCompress(this byte[] pBytes)
        {
            MemoryStream mMemory = new MemoryStream();
            Deflater mDeflater = new Deflater(Deflater.BEST_COMPRESSION);
            using (DeflaterOutputStream mStream = new DeflaterOutputStream(mMemory, mDeflater, 131072))
            {
                mStream.Write(pBytes, 0, pBytes.Length);
            }

            return mMemory.ToArray();
        }

        /// <summary>
        /// 压缩算法
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static string FlaterDeCompressString(this string base64String)
        {
            if (string.IsNullOrEmpty(base64String) || base64String.Length == 0) return "";
            var enbuf = Convert.FromBase64String(base64String);
            var debuf = FlaterDeCompress(enbuf);
            return Encoding.UTF8.GetString(debuf);
        }

        /// <summary>
        /// 解压缩算法
        /// </summary>
        /// <param name="pBytes"></param>
        /// <returns></returns>
        public static byte[] FlaterDeCompress(this byte[] pBytes)
        {
            MemoryStream mMemory = new MemoryStream();
            using (InflaterInputStream mStream = new InflaterInputStream(new MemoryStream(pBytes)))
            {
                Int32 mSize;
                byte[] mWriteData = new byte[4096];
                while (true)
                {
                    mSize = mStream.Read(mWriteData, 0, mWriteData.Length);
                    if (mSize > 0)
                        mMemory.Write(mWriteData, 0, mSize);
                    else
                        break;
                }
            }
            return mMemory.ToArray();
        }
        #endregion

        #region "Gzip 压缩"
        /// <summary>
        /// 根据DATASET压缩字符串
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static string GetStringByDataset(string ds)
        {
            return GZipCompressString(ds);
        }

        /// <summary>
        /// 将传入字符串以GZip算法压缩后，返回Base64编码字符
        /// </summary>
        /// <param name="rawString">需要压缩的字符串</param>
        /// <returns>压缩后的Base64编码的字符串</returns>
        public static string GZipCompressString(this string rawString)
        {
            if (string.IsNullOrEmpty(rawString) || rawString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] rawData = Encoding.UTF8.GetBytes(rawString);
                byte[] zippedData = GzipCompress(rawData);
                return Convert.ToBase64String(zippedData);
            }
        }
        /// <summary>
        /// GZip压缩
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static byte[] GzipCompress(this byte[] rawData)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
            compressedzipStream.Write(rawData, 0, rawData.Length);
            compressedzipStream.Close();
            return ms.ToArray();
        }
        #endregion

        #region "Gzip 解压"

        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static DataSet GetDatasetByString(string Value)
        {
            DataSet ds = new DataSet();
            string CC = GZipDeCompressString(Value);
            StringReader Sr = new StringReader(CC);
            ds.ReadXml(Sr);
            return ds;
        }

        /// <summary>
        /// 将传入的二进制字符串资料以GZip算法解压缩
        /// </summary>
        /// <param name="zippedString">经GZip压缩后的Base64字符串</param>
        /// <returns>原始未压缩字符串</returns>
        public static string GZipDeCompressString(this string zippedString)
        {
            if (string.IsNullOrEmpty(zippedString) || zippedString.Length == 0)
            {
                return "";
            }
            else
            {   
                //if(zippedString.Length % 4 != 0)
                //{
                //    zippedString += new string('=', 4 - (zippedString.Length % 4));
                //}
                //Console.WriteLine(zippedString.Length+ "\"" + Encoding.UTF8.GetBytes(zippedString).ToHexString() + "\"");
                byte[] zippedData = Convert.FromBase64String(zippedString);
                return Encoding.UTF8.GetString(GzipDeCompress(zippedData));
            }
        }
        /// <summary>
        /// ZIP解压
        /// </summary>
        /// <param name="zippedData"></param>
        /// <returns></returns>
        public static byte[] GzipDeCompress(this byte[] zippedData)
        {
            MemoryStream ms = new MemoryStream(zippedData);
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Decompress);
            MemoryStream outBuffer = new MemoryStream();
            byte[] block = new byte[65536];//64k缓冲块
            while (true)
            {
                int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                else
                    outBuffer.Write(block, 0, bytesRead);
            }
            compressedzipStream.Close();
            return outBuffer.ToArray();
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// DES 加密
/// </summary>
public class DESHelper
{

    string _iv = "linyee100";
    string _key = "12345678";

    /// <summary>
    /// 创建加密对象
    /// </summary>
    public DESHelper()
    {
        initkeyiv();
    }

    /// <summary>
    /// 创建加密对象
    /// </summary>
    /// <param name="key"></param>
    /// <param name="iv"></param>
    public DESHelper(string key,string iv)
    {
        this.Key = key;
        this.Iv = iv;
    }

    /// <summary>
    /// DES加密偏移量，必须是>=8位长的字符串
    /// </summary>
    public string Iv
    {
        get { return _iv; }
        set { _iv = value;
            initkeyiv();
        }
    }

    /// <summary>
    /// DES加密的私钥，必须是8位长的字符串
    /// </summary>
    public string Key
    {
        get { return _key; }
        set { _key = value;
            initkeyiv();
        }
    }

    /// <summary>
    /// key md5 后8字节，iv md5 16字节+key前8字节
    /// </summary>
    private void initkeyiv()
    {
        initkeyiv2();
        //return;

        //MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
        //byte[] mdtkey = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(_key));
        //byte[] md5iv = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(_iv));

        //byte[] riv = new byte[24];
        //byte[] rkey = new byte[8];
        //Array.Copy(md5iv, 0, riv, 0, 16);
        //Array.Copy(mdtkey, 0, riv, 16, 8);
        ////RealKey = Encoding.UTF8.GetBytes(_key);
        //Array.Copy(mdtkey, 8, rkey, 0, 8);
        //RealIv = riv;
        //RealKey = rkey;
        ////RealIv = Encoding.UTF8.GetBytes(_iv);
    }

    /// <summary>
    /// key+iv md5 后8字节key 前8字节iv
    /// </summary>
    private void initkeyiv2()
    {
        MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
        byte[] mdtkey = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(_key+_iv));

        byte[] riv = new byte[8];
        byte[] rkey = new byte[8];
        Array.Copy(mdtkey, 0, riv, 0, 8);
        Array.Copy(mdtkey, 8, rkey, 0, 8);
        RealIv = riv;
        RealKey = rkey;
    }

    /// <summary>
    /// 
    /// </summary>
    public byte[] RealKey { get; private set; }
    /// <summary>
    /// 
    /// </summary>
    public byte[] RealIv { get; private set; }

    /// <summary>
    /// 加密返回WEB使用的数据
    /// </summary>
    /// <param name="sourceString"></param>
    /// <returns></returns>
    public string EncryptWeb(string sourceString)
    {
        return Encrypt(sourceString).Replace("+", "%2b");
    }

    /// <summary>
    /// 对字符串进行DES加密
    /// </summary>
    /// <param name="sourceString">待加密的字符串</param>
    /// <returns>加密后的BASE64编码的字符串</returns>
    public string Encrypt(string sourceString)
    {
        byte[] btKey = RealKey;// Encoding.Default.GetBytes(_key);
        byte[] btIv = RealIv;// Encoding.Default.GetBytes(_iv);

        //LogService.Runtime(btKey.Length+","+ btIv .Length+ "");

        var des = new DESCryptoServiceProvider();
        des.Mode = CipherMode.CBC;
        des.Padding = PaddingMode.PKCS7;

        using (var ms = new MemoryStream())
        {
            byte[] inData = Encoding.UTF8.GetBytes(sourceString);
            try
            {
                using (var cs = new CryptoStream(ms, des.CreateEncryptor(btKey, btIv), CryptoStreamMode.Write))
                {
                    cs.Write(inData, 0, inData.Length);
                    cs.FlushFinalBlock();
                }

                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                LogService.Exception(ex);
                throw ex;
            }
        }
    }

    /// <summary>
    /// 对DES加密后的字符串进行解密
    /// </summary>
    /// <param name="encryptedString">待解密的字符串</param>
    /// <returns>解密后的字符串</returns>
    public string Decrypt(string encryptedString)
    {
        byte[] btKey = RealKey;// Encoding.Default.GetBytes(_key);
        byte[] btIv = RealIv;// Encoding.Default.GetBytes(_iv);
        var des = new DESCryptoServiceProvider();
        des.Mode = CipherMode.CBC;
        des.Padding = PaddingMode.PKCS7;

        using (var ms = new MemoryStream())
        {
            try
            {
                byte[] inData = Convert.FromBase64String(encryptedString);
                using (var cs = new CryptoStream(ms, des.CreateDecryptor(btKey, btIv), CryptoStreamMode.Write))
                {
                    cs.Write(inData, 0, inData.Length);
                    cs.FlushFinalBlock();
                }

                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                LogService.Exception(ex);
                throw ex;
            }
        }
    }
}


/// <summary>
/// 类名称   ：CryptTools
/// 类说明   ：加解密算法
/// </summary>
public static class CryptTools
{
    /// <summary>
    /// 方法说明　：加密方法
    /// 作者    　： 
    /// 完成日期　：
    /// </summary>
    /// <param name="content">需要加密的明文内容</param>
    /// <param name="secret">加密密钥</param>
    /// <returns>返回加密后密文字符串</returns>
    public static string Encrypt(string content, string secret)
    {
        if ((content == null) || (secret == null) || (content.Length == 0) || (secret.Length == 0))
            throw new ArgumentNullException("Invalid Argument");

        byte[] Key = GetKey(secret);
        byte[] ContentByte = Encoding.Unicode.GetBytes(content);
        MemoryStream MSTicket = new MemoryStream();

        MSTicket.Write(ContentByte, 0, ContentByte.Length);

        byte[] ContentCryptByte = Crypt(MSTicket.ToArray(), Key);

        string ContentCryptStr = Encoding.ASCII.GetString(Base64Encode(ContentCryptByte));

        return ContentCryptStr;
    }

    /// <summary>
    /// 方法说明　：解密方法
    /// 作者    　： 
    /// 完成日期　：
    /// </summary>
    /// <param name="content">需要解密的密文内容</param>
    /// <param name="secret">解密密钥</param>
    /// <returns>返回解密后明文字符串</returns>
    public static string Decrypt(string content, string secret)
    {
        if ((content == null) || (secret == null) || (content.Length == 0) || (secret.Length == 0))
            throw new ArgumentNullException("Invalid Argument");

        byte[] Key = GetKey(secret);

        byte[] CryByte = Base64Decode(Encoding.ASCII.GetBytes(content));
        byte[] DecByte = Decrypt(CryByte, Key);

        byte[] RealDecByte;
        string RealDecStr;

        RealDecByte = DecByte;
        byte[] Prefix = new byte[Constants.Operation.UnicodeReversePrefix.Length];
        Array.Copy(RealDecByte, Prefix, 2);

        if (CompareByteArrays(Constants.Operation.UnicodeReversePrefix, Prefix))
        {
            byte SwitchTemp = 0;
            for (int i = 0; i < RealDecByte.Length - 1; i = i + 2)
            {
                SwitchTemp = RealDecByte[i];
                RealDecByte[i] = RealDecByte[i + 1];
                RealDecByte[i + 1] = SwitchTemp;
            }
        }

        RealDecStr = Encoding.Unicode.GetString(RealDecByte);
        return RealDecStr;
    }


    /// <summary>
    /// 使用TripleDES加密 ,三倍DES加密
    /// </summary>
    /// <param name="source"></param>
    /// <param name="key"></param>
    /// <returns></returns>

    public static byte[] Crypt(byte[] source, byte[] key)
    {
        if ((source.Length == 0) || (source == null) || (key == null) || (key.Length == 0))
        {
            throw new ArgumentException("Invalid Argument");
        }

        TripleDESCryptoServiceProvider dsp = new TripleDESCryptoServiceProvider();
        dsp.Mode = CipherMode.ECB;

        ICryptoTransform des = dsp.CreateEncryptor(key, null);

        return des.TransformFinalBlock(source, 0, source.Length);
    }



    //使用TripleDES解密 来处理，三倍DES解密
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static byte[] Decrypt(byte[] source, byte[] key)
    {
        if ((source.Length == 0) || (source == null) || (key == null) || (key.Length == 0))
        {
            throw new ArgumentNullException("Invalid Argument");
        }

        TripleDESCryptoServiceProvider dsp = new TripleDESCryptoServiceProvider();
        dsp.Mode = CipherMode.ECB;

        ICryptoTransform des = dsp.CreateDecryptor(key, null);

        byte[] ret = new byte[source.Length + 8];

        int num;
        num = des.TransformBlock(source, 0, source.Length, ret, 0);

        ret = des.TransformFinalBlock(source, 0, source.Length);
        ret = des.TransformFinalBlock(source, 0, source.Length);
        num = ret.Length;

        byte[] RealByte = new byte[num];
        Array.Copy(ret, RealByte, num);
        ret = RealByte;
        return ret;
    }

    //原始base64编码
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static byte[] Base64Encode(byte[] source)
    {
        if ((source == null) || (source.Length == 0))
            throw new ArgumentException("source is not valid");

        ToBase64Transform tb64 = new ToBase64Transform();
        MemoryStream stm = new MemoryStream();
        int pos = 0;
        byte[] buff;

        while (pos + 3 < source.Length)
        {
            buff = tb64.TransformFinalBlock(source, pos, 3);
            stm.Write(buff, 0, buff.Length);
            pos += 3;
        }

        buff = tb64.TransformFinalBlock(source, pos, source.Length - pos);
        stm.Write(buff, 0, buff.Length);

        return stm.ToArray();

    }

    //原始base64解码
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static byte[] Base64Decode(byte[] source)
    {
        if ((source == null) || (source.Length == 0))
            throw new ArgumentException("source is not valid");

        FromBase64Transform fb64 = new FromBase64Transform();
        MemoryStream stm = new MemoryStream();
        int pos = 0;
        byte[] buff;

        while (pos + 4 < source.Length)
        {
            buff = fb64.TransformFinalBlock(source, pos, 4);
            stm.Write(buff, 0, buff.Length);
            pos += 4;
        }

        buff = fb64.TransformFinalBlock(source, pos, source.Length - pos);
        stm.Write(buff, 0, buff.Length);
        return stm.ToArray();

    }

    /// <summary>
    /// 把密钥转化为2进制byte[] 如果大于 24byte就取前24位 作为 密钥
    /// </summary>
    /// <param name="secret"></param>
    /// <returns></returns>
    public static byte[] GetKey(string secret)
    {
        if ((secret == null) || (secret.Length == 0))
            throw new ArgumentException("Secret is not valid");

        byte[] temp;

        ASCIIEncoding ae = new ASCIIEncoding();
        temp = Hash(ae.GetBytes(secret));

        byte[] ret = new byte[Constants.Operation.KeySize];

        int i;

        if (temp.Length < Constants.Operation.KeySize)
        {
            System.Array.Copy(temp, 0, ret, 0, temp.Length);
            for (i = temp.Length; i < Constants.Operation.KeySize; i++)
            {
                ret[i] = 0;
            }
        }
        else
            System.Array.Copy(temp, 0, ret, 0, Constants.Operation.KeySize);

        return ret;
    }

    //比较两个byte数组是否相同
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    public static bool CompareByteArrays(byte[] source, byte[] dest)
    {
        if ((source == null) || (dest == null))
            throw new ArgumentException("source or dest is not valid");

        bool ret = true;

        if (source.Length != dest.Length)
            return false;
        else
            if (source.Length == 0)
            return true;

        for (int i = 0; i < source.Length; i++)
            if (source[i] != dest[i])
            {
                ret = false;
                break;
            }
        return ret;
    }

    //使用md5计算散列
    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static byte[] Hash(byte[] source)
    {
        if ((source == null) || (source.Length == 0))
            throw new ArgumentException("source is not valid");

        MD5 m = MD5.Create();
        return m.ComputeHash(source);
    }

    /// <summary>
    /// 对传入的明文密码进行Hash加密,密码不能为中文
    /// </summary>
    /// <param name="oriPassword">需要加密的明文密码</param>
    /// <returns>经过Hash加密的密码</returns>
    public static string HashPassword(string oriPassword)
    {
        if (string.IsNullOrEmpty(oriPassword))
            throw new ArgumentException("oriPassword is valid");

        ASCIIEncoding acii = new ASCIIEncoding();
        byte[] hashedBytes = Hash(acii.GetBytes(oriPassword));

        StringBuilder sb = new StringBuilder(30);
        foreach (byte b in hashedBytes)
        {
            sb.AppendFormat("{0:X2}", b);
        }
        return sb.ToString();
    }

    /// <summary>
    /// 注意:密钥必须为８位
    /// </summary>
    public static string m_strEncryptKey = "kingfykj";

    #region DES加密字符串
    /// <summary>
    /// 加密字符串 与 java 通用
    /// </summary>
    /// <param name="p_strInput">明码</param>
    /// <returns>加密后的密码</returns>
    public static string DesEncryptFixKey(string p_strInput)
    {
        if (string.IsNullOrEmpty(p_strInput))
            throw new ArgumentException("要加密的字符串不能为空");

        byte[] byKey = null;
        byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        try
        {
            byKey = System.Text.Encoding.UTF8.GetBytes(m_strEncryptKey.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(p_strInput);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }
        catch (System.Exception ex)
        {
            throw (ex);
        }
    }
    #endregion

    #region DES解密字符串
    /// <summary>
    /// 解密字符串 与 java 通用
    /// </summary>
    /// <param name="p_strInput"></param>
    public static string DesDecryptFixKey(string p_strInput)
    {
        byte[] byKey = null;
        byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        byte[] inputByteArray = new Byte[p_strInput.Length];

        try
        {
            byKey = System.Text.Encoding.UTF8.GetBytes(m_strEncryptKey.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            inputByteArray = Convert.FromBase64String(p_strInput);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            System.Text.Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetString(ms.ToArray());
        }
        catch (System.Exception ex)
        {
            throw (ex);
        }
    }
    #endregion

}

/// <summary>
/// 类名称   ：Constants
/// 类说明   ：加解密算法常量.
/// 作者     ：
/// 完成日期 ：
/// </summary>
public class Constants
{
    /// <summary>
    /// 
    /// </summary>
    public struct Operation
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly int KeySize = 24;
        /// <summary>
        /// 
        /// </summary>
        public static readonly byte[] UnicodeOrderPrefix = new byte[2] { 0xFF, 0xFE };
        /// <summary>
        /// 
        /// </summary>
        public static readonly byte[] UnicodeReversePrefix = new byte[2] { 0xFE, 0xFF };
    }
}

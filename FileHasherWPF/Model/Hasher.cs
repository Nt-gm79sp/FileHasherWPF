using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;


namespace FileHasherWPF.Model
{

    /// <summary>
    /// Hasher的抽象类
    /// </summary>
    public abstract class Hasher
    {
        public enum HashAlgos
        {
            MD5,
            SHA1,
            SHA256,
            SHA512
        }

        // 此处使用了自动属性，因而不再需要私有成员
        public string HashAlgo { get; }

        public string Input { get; }

        public string HashResult { get; protected set; }

        // 构造函数中的自动属性
        public Hasher(HashAlgos algo, string input)
        {
            HashAlgo = algo switch
            {
                HashAlgos.MD5 => "MD5",
                HashAlgos.SHA1 => "SHA1",
                HashAlgos.SHA256 => "SHA256",
                HashAlgos.SHA512 => "SHA512",
                _ => "SHA256",
            };
            Input = input;
            HashResult = "";
        }

        /// <summary>
        /// 将字节数组格式化到字符串
        /// </summary>
        /// <param name="b">byte型数组</param>
        /// <returns>去掉连接符后的十六进制数字符串</returns>
        protected static string FormatBytes(byte[] b)
        {
            // 该方法不是解码，而是将HEX“音译”到字符串，1A->"1A"
            string s = BitConverter.ToString(b);
            s = s.Replace("-", string.Empty);
            return s;
        }
    }

    /// <summary>
    /// 对字符串进行哈希计算
    /// </summary>
    public class StringHasher : Hasher
    {
        public StringHasher(HashAlgos algo, string input) : base(algo, input)
        {
            HashResult = GetStringHash(HashAlgo, Input);
        }

        public static string GetStringHash(string hashType, string s)
        {
            // 将字符串转为字节数组
            byte[] byteArr = Encoding.Default.GetBytes(s);
            // 方法HashAlgorithm.Create()直接以字符串作为参数来选择算法类型，非常方便
            // 目前版本中，SHA2家族算法默认由托管实现，SHA1与MD5由CSP实现，即Windows内置的受到FIPS即美国政府认证的安全实现
            // SHA2家族亦有CSP/Cng实现，不同实现的性能有待测试，暂不折腾
            HashAlgorithm hash = HashAlgorithm.Create(hashType);
            // 计算结果，并转为字符串返回
            byte[] result = hash.ComputeHash(byteArr);
            return FormatBytes(result);
        }
    }

    /// <summary>
    /// 对文件进行哈希计算（异步）
    /// </summary>
    public class FileHasher : Hasher
    {

        public FileHasher(HashAlgos algo, string input) : base(algo, input)
        {

        }
    }

}

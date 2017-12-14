using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Utils
{
    public static class GetHash
    {
        // 字符串
        public static string GetStringHash(string hashType, string s)
        {
            // 方法HashAlgorithm.Create()直接以字符串作为参数来选择算法类型，非常方便
            HashAlgorithm hash = HashAlgorithm.Create(hashType);
            // 将字符串转为字节数组
            byte[] byteArr = Encoding.Default.GetBytes(s);
            // 计算结果，并转为字符串返回
            byte[] result = hash.ComputeHash(byteArr);
            return FormatBytes(result);
        }

        // 文件
        public static string GetFileHash(string hashType, FileStream f)
        {
            HashAlgorithm hash = HashAlgorithm.Create(hashType);
            byte[] result = hash.ComputeHash(f);
            return FormatBytes(result);
        }

        // 将字节数组格式化到字符串
        private static string FormatBytes(byte[] b)
        {
            string s = BitConverter.ToString(b); // 该方法不是解码，而是将HEX翻译到字符串，1A=>"1A"
            s = s.Replace("-", "");
            return s;
        }
    }
}

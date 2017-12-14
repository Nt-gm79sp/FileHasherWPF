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
            // 将字符串转为字节数组
            byte[] byteArr = Encoding.Default.GetBytes(s);
            // 方法HashAlgorithm.Create()直接以字符串作为参数来选择算法类型，非常方便
            HashAlgorithm hash = HashAlgorithm.Create(hashType);
            // 计算结果，并转为字符串返回
            byte[] result = hash.ComputeHash(byteArr);
            return FormatBytes(result);
        }

        // 文件
        public struct FileResult
        {
            public string path;
            public string hash;
        }
        public const string FILE_ERROR = "文件读取错误！";

        public static FileResult GetFileHash(string hashType, string filePath, bool isFullPath)
        {
            FileResult result;
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                HashAlgorithm hashCode = HashAlgorithm.Create(hashType);
                byte[] hashCodeBytes = hashCode.ComputeHash(fs);
                result.hash = FormatBytes(hashCodeBytes);
            }
            catch
            {
                result.hash = FILE_ERROR;
            }
            finally
            {
                // 是否显示完整路径
                result.path = isFullPath ? filePath : Path.GetFileName(filePath);
            }
            return result;
        }

        // 将字节数组格式化到字符串
        private static string FormatBytes(byte[] b)
        {
            // 该方法不是解码，而是将HEX“音译”到字符串，1A=>"1A"
            string s = BitConverter.ToString(b);
            s = s.Replace("-", string.Empty);
            return s;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Utils
{
    /// <summary>
    /// 处理基本的字符串哈希与格式化
    /// </summary>
    public class GetHash
    {
        // 字符串
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

        // 将字节数组格式化到字符串
        protected static string FormatBytes(byte[] b)
        {
            // 该方法不是解码，而是将HEX“音译”到字符串，1A->"1A"
            string s = BitConverter.ToString(b);
            s = s.Replace("-", string.Empty);
            return s;
        }
    }

    /// <summary>
    /// 处理文件哈希过程，并返回进度与结果
    /// </summary>
    public class GetFileHash : GetHash
    {
        public string HashType { get; }
        public string FilePath { get; }

        public string FileName { get; }
        public long FileLength { get; }

        // 基于文件读取进度的进度报告
        public double Progress => fs.Length == 0 ? 100 : (fs.Position / fs.Length) * 100;

        public string HashResult { get; private set; }

        private FileStream fs;

        public const string FILE_ERROR = "文件读取错误！";
        public const string HASH_INCOMPL = "文件哈希取消";

        /// <summary>
        /// 进行文件哈希
        /// </summary>
        /// <param name="hashType">指定哈希类型，以字符串表示</param>
        /// <param name="filePath">完整的文件路径</param>
        public GetFileHash(string hashType, string filePath)
        {
            HashType = hashType;
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            HashResult = string.Empty;

            if (File.Exists(FilePath))
            {
                try
                {
                    // 以只读模式打开，不指定进程共享（独占）参数、异步读取参数
                    // 因为写在try中，所以不必using(){}的用法
                    fs = File.OpenRead(FilePath);
                    FileLength = fs.Length;
                    HashAlgorithm hash = HashAlgorithm.Create(HashType);
                    byte[] result = hash.ComputeHash(fs);
                    HashResult = FormatBytes(result);
                }
                catch
                {
                    HashResult = FILE_ERROR;
                    FileLength = 0L;
                }
                finally
                {
                    // 读取过程中并不隐含Dispose()方法，但是GC会自动回收（有延迟）
                    fs?.Dispose();
                }
            }
            else
            {
                HashResult = FILE_ERROR;
            }
        }

    }

}

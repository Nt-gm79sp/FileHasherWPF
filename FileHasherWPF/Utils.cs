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
        public string HashResult { get; private set; }

        private FileStream FS { get; }

        public const string FILE_ERROR = "文件读取错误！";
        public const string HASH_INCOMPL = "已取消文件读取";

        // 文件的读取进度
        // 无法直接得知IDisposable是否已被Dispose()，可catch异常，或额外用个bool挂旗
        public long GetCurrentBytesPosition()
        {
            try
            {
                return FS.Position;
                // 直接用ProgressBar的最大值来设置了，就不换算百分比了
                // return FileLength == 0 ? 100 : (fs.Position / FileLength) * 100;
            }
            catch
            {
                return FileLength;
            }
        }

        /// <summary>
        /// 进行文件哈希
        /// </summary>
        /// <param name="hashType">指定哈希类型，以字符串表示</param>
        /// <param name="filePath">完整的文件路径</param>
        public GetFileHash(string hashType, string filePath)
        {
            HashType = hashType;
            FilePath = filePath;
            // 获取文件名是纯字符串操作，不会抛出文件系统异常。错误的文件名返回空串
            FileName = Path.GetFileName(filePath);
            HashResult = HASH_INCOMPL;

            if (File.Exists(FilePath))
            {
                try
                {
                    // 以只读模式打开，不指定进程共享（独占）参数、异步读取参数
                    // 因为写在try中，所以不必using(){}的用法
                    FS = File.OpenRead(FilePath);
                    FileLength = FS.Length;
                    HashAlgorithm hash = HashAlgorithm.Create(HashType);
                    byte[] result = hash.ComputeHash(FS);
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
                    FS?.Dispose();
                }
            }
            else
            {
                HashResult = FILE_ERROR;
            }
        }
        // 另一种Dispose方式，带个bool
        //class MyClient : TcpClient
        //{
        //    public bool IsDead { get; set; }
        //    protected override void Dispose(bool disposing)
        //    {
        //        IsDead = true;
        //        base.Dispose(disposing);
        //    }
        //}
    }

}

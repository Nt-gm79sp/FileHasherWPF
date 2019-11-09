using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Utils
{
    public static class ConstStrings
    {
        public const string FILE_ERROR = "文件读取错误！";
        public const string HASH_INCOMPL = "已取消文件读取";
        public const string HASH_EQUAL = "校验值相同";
        public const string SUCCESS = "恭喜";
        public const string HASH_UNEQUAL = "校验值不同！";
        public const string CAUTION = "注意！";
    }

    /// <summary>
    /// 处理基本的字符串哈希与格式化
    /// </summary>
    public static class GetHash
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
        public static string FormatBytes(byte[] b)
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
    public class GetFileHash
    {
        public string FilePath { get; }
        public string FileName { get; }
        public long FileLength { get; }
        public string HashType { get; }
        public string HashResult { get; private set; }

        private FileStream FS { get; }

        private const string FILE_ERROR = ConstStrings.FILE_ERROR;
        private const string HASH_INCOMPL = ConstStrings.HASH_INCOMPL;

        /// <summary>
        /// 初始化文件哈希计算
        /// </summary>
        /// <param name="hashType">指定哈希类型，以字符串表示</param>
        /// <param name="filePath">文件路径</param>
        public GetFileHash(string hashType, string filePath)
        {
            FilePath = filePath;
            HashType = hashType;
            // 获取文件名是纯字符串操作，不会抛出文件系统异常。错误的文件名返回空串
            FileName = Path.GetFileName(filePath);
            HashResult = HASH_INCOMPL;
            try
            {
                // 以只读模式打开，不指定进程共享（独占）参数、异步读取参数
                // 因为写在try中，所以不必using(){}的用法，但需要注意Dispose()
                FS = File.OpenRead(filePath);
                //FS = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
                FileLength = FS.Length;
            }
            catch
            {
                HashResult = FILE_ERROR;
                FileLength = 0L;
                // 读取过程中并不隐含Dispose()方法，但是GC会自动回收（有延迟）
                FS?.Dispose();
            }
        }

        /// <summary>
        /// 开始计算文件哈希值
        /// </summary>
        public async Task StartHash()
        {
            if (FS != null && HashResult != FILE_ERROR)
            {
                // 异步的标准用法之一，很关键哦
                await Task.Run(() =>
                {
                    try
                    {
                        HashAlgorithm hash = HashAlgorithm.Create(HashType);
                        byte[] result = hash.ComputeHash(FS);
                        HashResult = GetHash.FormatBytes(result);
                    }
                    catch { }
                    finally
                    {
                        FS?.Dispose();
                    }
                });
            }
        }

        /// <summary>
        /// 取消当前任务（如果任务存在）
        /// </summary>
        public void Stop()
        {
            // 这里的写法非常简单粗暴，直接关闭文件流，忽略异常
            // 正常的写法应当是使用 CancellationTokenSource 及其 Token，
            // 在循环中使用buffer读取文件，在CTS.Cancel()后跳出循环
            if (FS != null && HashResult == HASH_INCOMPL)
                FS.Dispose();
        }

        /// <summary>
        /// 获取当前文件读取字节位置，如异常则返回文件长度（默认为0）
        /// </summary>
        public long GetCurrentBytesPosition()
        {
            // 无法直接得知IDisposable是否已被Dispose()，可catch异常，
            // 或额外用个bool挂旗，或进一步override Dispose()方法等
            try
            {
                return FS.Position;
            }
            catch
            {
                return FileLength;
            }
        }

    }

}

# FileHasherWPF

简单方便的文件哈希工具 Quick Tool for Hashing Files

## 使用

![](https://user-images.githubusercontent.com/6905026/34327126-a5c54b66-e8f7-11e7-8362-530eb011038a.png)

直接将文件拖入窗口即可，支持多个文件。

## 关于代码

Translation can be done easily. Strings are extracted.

平时经常会计算文件哈希值，但又不是每次都值得开个大家伙来完成，索性写了个小工具，顺便学习一下WPF。

本例中的核心代码其实就几行：
```csharp
FileStream fs = File.OpenRead(filePath); //打开文件
HashAlgorithm hash = HashAlgorithm.Create(hashType); //建立哈希对象
byte[] result = hash.ComputeHash(fs); //计算并返回结果
return BitConverter.ToString(result).Replace("-", string.Empty); //将二进制结果转换成字符串
```
剩下一大堆便是围绕这几行代码展开的。

#### 一些可改进的地方

- 用到了WPF的XAML布局，但没做数据绑定
- 队列机制与取消机制比较简单粗暴，没怎么利用Task（WhenAll、WhenAny、CancellationToken，etc.）
- 如果做了数据绑定，可以进一步利用事件机制
- 进一步提取文本并生成多语言资源等

不过既然核心功能表现良好，就不折腾了。学习的事情，留给新项目吧。

#### .NET Core 3.0 WPF

代码已迁移到.NET Core 3.0，纯属尝鲜，因为程序简单所以没有遇到什么困难。

但是.NET Core 3.0生成出来的EXE文件较大（~158KiB），且需要附带DLL、PDB、JSON文件（~32KiB）。如果自带运行时而打包成单个EXE，则颇为硕大（~85MiB）。

相比之下，基于.NET Framework生成的文件只有27KiB，且启动更快，所以目前发布下载的仍然是老版本EXE。

期待未来的.NET 5时代。

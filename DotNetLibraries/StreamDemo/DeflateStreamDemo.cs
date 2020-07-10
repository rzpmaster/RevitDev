using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDemo
{
    /// <summary>
    /// 压缩流 DeflateStream 在
    /// GZipStream(内部使用DeflateStream，但是增加了循环冗余校验，检测数据损坏情况) 
    /// ZipArchive(创建读取ZIP文件)
    /// </summary>
    class DeflateStreamDemo
    {
        public void DecompressFile()
        {
            //创建文件读取流
            FileStream inputStream = File.OpenRead(compressFileName);
            //这里为了直接输出文件内容使用了MemoryStream，可以换成FileStream用来保存解压后的文件
            using (MemoryStream outputStream = new MemoryStream())
            {
                //解压读取的文件
                using (var compressStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    //将解压流写入到内存流中，以便后续直接输出
                    compressStream.CopyTo(outputStream);
                    //定位内存流的当前位置,因为读完之后流的Position在最后
                    outputStream.Seek(0, SeekOrigin.Begin);

                    //将内存流的内容使用StreamReader输出文本
                    using (var reader = new StreamReader(outputStream,
                        Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: true,
                        bufferSize: 4096,
                        //注意：此参数很有用途,如果在释放 System.IO.StreamReader 对象后保持流处于打开状态，则为 true；否则为 false。这里可以是true是因为最外面的MemoryStream在using结束后会销毁，如果在后面的代码中需要用到的话，这里就不许为true。
                        leaveOpen: true))
                    {
                        string result = reader.ReadToEnd();
                        Console.WriteLine(result);
                    }
                }
            }
        }

        public void CompressFile()
        {
            //读取要压缩的文件
            using (FileStream inputStream = File.OpenRead(tempTextFileName))
            {
                //创建要写入的文件
                FileStream outputStream = File.OpenWrite(compressFileName);

                //创建压缩流，构造方法指明最终写入的文件流
                using (var compressStream = new DeflateStream(outputStream, CompressionMode.Compress))
                {
                    //将读取的文件流写入到压缩流中
                    inputStream.CopyTo(compressStream);
                }
            }
        }

        string tempTextFileName = Program.tempTextFileName;     //源文件
        string compressFileName = Path.Combine(Path.GetDirectoryName(Program.tempTextFileName1), "compress.ys");   //压缩文件
    }

    /// <summary>
    /// 可以使用ZipArchive创建Zip压缩文件。
    /// ZipArchive包含多个ZipArchiveEntry对象，ZipArchive类不是一个流，但是它使用流进行读写。
    /// 
    /// ZipArchive位于System.IO.Compression命名空间下
    /// </summary>
    class ZipArchiveDemo
    {
        public void CreateZipFile()
        {
            //将要穿件的压缩文件对应的写入流
            FileStream zipStream = File.OpenWrite(ZipFilePath);

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                //获取目录下的所有文件
                IEnumerable<string> files = Directory.EnumerateFiles(DirectoryPath, "*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    //针对每个文件创建ZipArchiveEntry对象
                    ZipArchiveEntry entry = archive.CreateEntry(Path.GetFileName(file));

                    //创建每个文件对应的读取流
                    using (FileStream inputSream = File.OpenRead(file))
                    {
                        //打开ZipArchiveEntry对象的压缩流
                        using (Stream outputStream = entry.Open())
                        {
                            //将文件写入到压缩流中
                            inputSream.CopyTo(outputStream);
                        }
                    }
                    
                }
            }
        }

        string DirectoryPath
        {
            get
            {
                string directoryPath = Path.Combine(Path.GetDirectoryName(Program.tempTextFileName), "temp");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                //在文件夹中放一些文件
                var files = Directory.GetFiles(directoryPath);
                if (files.Length == 0)
                {
                    using (var fileStream = File.Create(Path.Combine(directoryPath, "temp.txt")))
                    {
                        byte[] preamble = Encoding.UTF8.GetPreamble();
                        fileStream.Write(preamble, 0, preamble.Length);
                        string hello = "你好，C#！";
                        byte[] buffer = Encoding.UTF8.GetBytes(hello);
                        fileStream.Write(buffer, 0, buffer.Length);
                    }
                }

                return directoryPath;
            }
        }

        string ZipFilePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(Program.tempTextFileName), "temp.zip");
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDemo
{
    class Program
    {
        static string deskPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static string tempTextFileName = Path.Combine(deskPath, "temp.txt");     //读取文件
        public static string tempTextFileName1 = Path.Combine(deskPath, "temp1.txt");   //写入文件

        static void Main(string[] args)
        {
            #region FileStream
            //FileStreamDemo fileStreamDemo = new FileStreamDemo();

            //fileStreamDemo.ReadFileUsingFileStream();
            //fileStreamDemo.WriteTextFile();
            //fileStreamDemo.CopyUsingStream2();
            //fileStreamDemo.CreateSampleFileAsync();
            //fileStreamDemo.RandomAccessSample(); 
            #endregion

            #region StreamReader
            //StreamReaderDemo streamReaderDemo = new StreamReaderDemo();
            //streamReaderDemo.ReadFileUsingReader();
            //streamReaderDemo.WriteFileUsingWriter("Hello,World!");
            #endregion

            #region BinaryReader
            //BinaryReaderDemo binaryReaderDemo = new BinaryReaderDemo();
            //binaryReaderDemo.WriteFileUsingBinaryWriter();
            //binaryReaderDemo.ReadFileUsingBinaryReader();
            #endregion

            #region DeflateStream
            //DeflateStreamDemo deflateStreamDemo = new DeflateStreamDemo();
            //deflateStreamDemo.CompressFile();
            //deflateStreamDemo.DecompressFile();
            #endregion

            #region ZipArchive
            //ZipArchiveDemo zipArchiveDemo = new ZipArchiveDemo();
            //zipArchiveDemo.CreateZipFile();
            #endregion

            #region FileSystemWatcherDemo
            //FileSystemWatcherDemo.WatchFiles("./", "*");
            #endregion






            ConvertStringAndByteArray();

            Console.ReadLine();
        }

        static void ConvertStringAndByteArray()
        {
            string str = "你好吗？好好学习，天天向上！";

            //将字符串转换为字节
            byte[] bs = Encoding.UTF8.GetBytes(str);

            //将字节数组转换为等效字符串，实际应用中可以存储该字符串到文件中
            string abc = Convert.ToBase64String(bs);
            Console.WriteLine("将字节数组转换为字符串:");
            Console.WriteLine(abc);

            //将等效字符串还原为数组
            byte[] bs2 = Convert.FromBase64String(abc);

            //将字节数组转换为字符串
            Console.WriteLine("还原后的结果：");
            Console.WriteLine(Encoding.UTF8.GetString(bs2));
        }
    }
}

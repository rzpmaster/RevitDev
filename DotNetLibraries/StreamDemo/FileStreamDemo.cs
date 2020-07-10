using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDemo
{
    /// <summary>
    /// 使用时需要注意 
    /// 字节数组
    /// 编码
    /// 流当前的位置
    /// </summary>
    class FileStreamDemo
    {
        public void RandomAccessSample()
        {
            //每次读取的字节数
            const int RECORDSIZE = 92;
            try
            {
                using (FileStream stream = File.OpenRead(tempTextFileName))
                {
                    byte[] buffer = new byte[RECORDSIZE];
                    do
                    {
                        try
                        {
                            Console.WriteLine("record numer (or 'bye' to end):");
                            //接收用户输入的行号
                            string line = Console.ReadLine();
                            if ("BYE".Equals(line.ToUpper())) break;

                            int record;
                            if (int.TryParse(line, out record))
                            {
                                //设定流的读取位置
                                stream.Seek((record - 1) * RECORDSIZE, SeekOrigin.Begin);
                                stream.Read(buffer, 0, RECORDSIZE);
                                string s = Encoding.UTF8.GetString(buffer);
                                Console.WriteLine("record:\n" + s);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    } while (true);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("文件不存在！");
            }
        }

        public async void CreateSampleFileAsync()
        {
            int nRecords = 1000;

            //虽然FileStream没有使用using或者调用释放方法，但是在StreamWriter销毁时，StreamWriter会控制所使用的资源，并销毁流
            FileStream stream = File.Create(tempTextFileName);

            //StreamWriter用于文本流的写入
            using (var writer = new StreamWriter(stream))
            {
                var r = new Random();
                var records = Enumerable.Range(0, nRecords).Select(x => new
                {
                    Number = x,
                    Text = $"Sample text {r.Next(200)}",
                    Date = new DateTime(Math.Abs((long)((r.NextDouble() * 2 - 1) * DateTime.MaxValue.Ticks)))
                });

                foreach (var rec in records)
                {
                    string date = rec.Date.ToString("d", CultureInfo.InvariantCulture);
                    string s = $"#{rec.Number,8}; {rec.Text,-20}; {date}#{Environment.NewLine}";
                    await writer.WriteAsync(s);
                }
            }
        }

        public void CopyUsingStream2()
        {
            string inputFile = tempTextFileName;
            string outputFile = tempTextFileName1;

            using (var inputStream = File.OpenRead(inputFile))
            {
                using (var outputStream = File.OpenWrite(outputFile))
                {
                    //从当前流中读取字节并将其写入到另一流中
                    inputStream.CopyTo(outputStream);
                }
            }
        }

        public void WriteTextFile()
        {
            //在temp文件夹随机创建一个不重名的xxx.tmp文件
            //string tempTextFileName = Path.ChangeExtension(Path.GetTempFileName(), "txt");

            //File.OpenWrite 默认追加写入
            using (FileStream stream = File.OpenWrite(tempTextFileName))
            {
                //在写入文本之前，需要先写入序言信息
                //返回使用UTF-8的字节序列，preamble即为编码为UTF-8的字节序列，它代表文件的序言信息
                byte[] preamble = Encoding.UTF8.GetPreamble();
                //写入序言信息为UTF-8到文件流中
                stream.Write(preamble, 0, preamble.Length);

                string hello = "你好，C#！";
                //将字符串转换为字节数组写入到文件流中
                byte[] buffer = Encoding.UTF8.GetBytes(hello);
                stream.Write(buffer, 0, buffer.Length);
                Console.WriteLine("文件:" + stream.Name + "，已经写入");
            }
        }

        public void ReadFileUsingFileStream()
        {
            //每次读取的字节数
            const int BUFFERSIZE = 4096;
            using (var stream = new FileStream(tempTextFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                ShowStreamInfomation(stream);
                Encoding encoding = GetEncoding(stream);

                byte[] buffer = new byte[BUFFERSIZE];
                bool completed = false;
                do
                {
                    int nread = stream.Read(buffer, 0, BUFFERSIZE);
                    //如果没有字节被读取，就跳出循环
                    if (nread == 0) break;
                    //如果最终读取的字节数小于缓冲区的字节数大小
                    if (nread < BUFFERSIZE)
                    {
                        //Clear()方法用于清除数组中不需要的多余元素
                        //将buffer数组从索引nread处开始清理，清理的元素个数为BUFFERSIZE - nread
                        //这样就将未填满的buffer中，多余的字节给清理调用
                        Array.Clear(buffer, nread, BUFFERSIZE - nread);
                        completed = true;
                    }
                    Console.WriteLine($"读取 {nread} 字节");
                    //将读取到缓冲区buffer中的数据，按照指定的编码格式，转换为字符串并输出
                    //此处可以直接调用encoding.GetString(buffer)
                    string s = encoding.GetString(buffer, 0, nread);
                    Console.WriteLine(s);
                } while (!completed);
            }
        }

        private void ShowStreamInfomation(FileStream stream)
        {
            Console.WriteLine("当前流是否可读取:" + stream.CanRead);
            Console.WriteLine("当前流是否可写入:" + stream.CanWrite);
            Console.WriteLine("当前流是否支持搜索:" + stream.CanSeek);
            Console.WriteLine("当前流是否可以超时:" + stream.CanTimeout);
            Console.WriteLine("当前流长度:" + stream.Length);
            Console.WriteLine("当前流的位置:" + stream.Position);
            //如果可以超时
            if (stream.CanTimeout)
            {
                Console.WriteLine("流在尝试读取多少毫秒后超时:" + stream.ReadTimeout);
                Console.WriteLine("流在尝试写入多少毫秒后超时:" + stream.WriteTimeout);
            }
        }

        /// <summary>
        /// 从前5个字节（序言）获取获取文件编码，也称为字节顺序标记（Byte Order Mark，BOM）
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private Encoding GetEncoding(FileStream stream)
        {
            //如果当前流不支持检索就抛出异常
            if (!stream.CanSeek) throw new ArgumentException("require a stream that can seek");

            Encoding encoding = Encoding.ASCII;
            //定义缓冲区，这里只是为了将BOM格式写入到该字节数组中
            byte[] bom = new byte[5];
            //从流中读取字节块，填充bom字节数组的同时，返回读入缓冲区的总字节数
            //注意流可能小于缓冲区。如果没有更多的字符可用于读取，Read()方法就返回0，此时没有数据写入到缓冲区
            int nRead = stream.Read(bom, offset: 0, count: 5);

            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0)
            {
                Console.WriteLine("UTF-32");
                //将该流的当前位置设置为给定值，从流的开始位置起
                stream.Seek(4, SeekOrigin.Begin);
                return Encoding.UTF32;
            }
            else if (bom[0] == 0xff && bom[1] == 0xfe)
            {
                Console.WriteLine("UTF-16, little endian");
                stream.Seek(2, SeekOrigin.Begin);
                return Encoding.Unicode;
            }
            else if (bom[0] == 0xfe && bom[1] == 0xff)
            {
                Console.WriteLine("UTF-16,big endian");
                stream.Seek(2, SeekOrigin.Begin);
                return Encoding.BigEndianUnicode;
            }
            else if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
            {
                Console.WriteLine("UTF-8");
                stream.Seek(3, SeekOrigin.Begin);
                return Encoding.UTF8;
            }
            stream.Seek(0, SeekOrigin.Begin);
            return encoding;
        }

        string tempTextFileName = Program.tempTextFileName;
        string tempTextFileName1 = Program.tempTextFileName1;
    }
}

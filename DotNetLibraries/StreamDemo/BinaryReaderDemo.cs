using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDemo
{
    /// <summary>
    /// 对二进制进行读写，不需要编码！
    /// </summary>
    class BinaryReaderDemo
    {
        public void WriteFileUsingBinaryWriter()
        {
            var outputStream = File.Create(tempTextFileName);
            using (var writer = new BinaryWriter(outputStream))
            {
                double d = 47.47;
                int i = 42;
                long l = 987654321;
                string s = "sample";
                writer.Write(d);
                writer.Write(i);
                writer.Write(l);
                writer.Write(s);
            }
        }

        string tempTextFileName = @"C:\Users\rzp\Desktop\temp.txt";
    }
}

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
        public void ReadFileUsingBinaryReader()
        {
            var inputStream = File.Open(tempTextFileName, FileMode.Open);
            using (var reader = new BinaryReader(inputStream))
            {
                //读取并定位
                double d = reader.ReadDouble();
                int i = reader.ReadInt32();
                long l = reader.ReadInt64();
                string s = reader.ReadString();
                Console.WriteLine($"d:{d} \t i:{i} \t l:{l} \t s:{s} ");
            }
        }

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

        string tempTextFileName = Program.tempTextFileName;
    }
}

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
            StreamReaderDemo streamReaderDemo = new StreamReaderDemo();

            //streamReaderDemo.ReadFileUsingReader();

            streamReaderDemo.WriteFileUsingWriter("Hello,World!");
            #endregion

            Console.ReadLine();
        }

        
    }
}

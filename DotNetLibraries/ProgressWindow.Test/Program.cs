using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ProgressWindow.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            /// 简单进度条
            /// 

            using (ProgressInvoker progressInvoker = new ProgressInvoker(true))
            {
                progressInvoker.LaunchProgressWindow();
                progressInvoker.SetupTitle("Title...");
                progressInvoker.UpdateMainTipInfo("test...");

                // 耗时代码...
            }

            /// 复杂进度条
            /// 

            //ProgressInvoker progressInvoker = new ProgressInvoker();
            ////progressInvoker.OwnerPtrNum=0;
            //progressInvoker.LaunchProgressWindow();
            //progressInvoker.SetupTitle("干嘛鸭？");
            //progressInvoker.SetupProgressWindowPosition(new Point(0, 0));

            //int mainCount = 4;
            //int subCount = 1000;

            ////progressInvoker.SetMainProgressCount(1);
            //for (int i = 0; i < mainCount; i++)
            //{
            //    progressInvoker.UpdateMainTipInfo($"想你鸭~~第{i + 1}轮想你，进行中...");

            //    //progressInvoker.SetSubProgressCount(1);
            //    for (int j = 0; j < subCount; j++)
            //    {
            //        progressInvoker.UpdateDetailInfo($"第{i + 1}轮，第{j + 1}次想你");
            //        progressInvoker.UpdateSubTipInfo($"第{i + 1}轮，第{j + 1}次想你");
            //        progressInvoker.UpdateSubProgress((j + 1) / 1.0 / subCount * 1.0);
            //    }

            //    progressInvoker.UpdateMainProgress((i + 1) / 1.0 / mainCount * 1.0);
            //}

            Console.ReadLine();
        }
    }
}

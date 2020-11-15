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
            //using (ProgressInvoker progressInvoker = new ProgressInvoker(true))
            //{
            //    progressInvoker.LaunchProgressWindow();
            //    progressInvoker.UpdateMainTipInfo("test..."); 
            //}

            ProgressInvoker progressInvoke = new ProgressInvoker();
            progressInvoke.LaunchProgressWindow();

            int count = 1000;

            progressInvoke.UpdateMainTipInfo("干嘛鸭?");
            progressInvoke.SetSubProgressCount(1);
            for (int i = 0; i < count; i++)
            {
                progressInvoke.StartOneIteration($"第{i}次想你~~");
                progressInvoke.UpdateDetailInfo($"1*第{i}次想你~~");
                progressInvoke.UpdateSubProgress((double)i / count);
            }
            progressInvoke.UpdateMainProgress(0.25);

            progressInvoke.UpdateMainTipInfo("干嘛鸭?");
            progressInvoke.SetSubProgressCount(1);
            for (int i = 0; i < count; i++)
            {
                progressInvoke.StartOneIteration($"第{i}次想你~~");
                progressInvoke.UpdateDetailInfo($"2*第{i}次想你~~");
                progressInvoke.UpdateSubProgress((double)i / count);
            }
            progressInvoke.UpdateMainProgress(0.5);

            progressInvoke.UpdateMainTipInfo("干嘛鸭?");
            progressInvoke.SetSubProgressCount(1);
            for (int i = 0; i < count; i++)
            {
                progressInvoke.StartOneIteration($"第{i}次想你~~");
                progressInvoke.UpdateDetailInfo($"3*第{i}次想你~~");
                progressInvoke.UpdateSubProgress((double)i / count);
            }
            progressInvoke.UpdateMainProgress(0.75);

            progressInvoke.UpdateMainTipInfo("干嘛鸭?");
            progressInvoke.SetSubProgressCount(1);
            for (int i = 0; i < count; i++)
            {
                progressInvoke.StartOneIteration($"第{i}次想你~~");
                progressInvoke.UpdateDetailInfo($"4*第{i}次想你~~");
                progressInvoke.UpdateSubProgress((double)i / count);
            }
            progressInvoke.UpdateMainProgress(1);

            progressInvoke.StartOneIteration($"第{count*4}次想你,想你完成~~");

            Console.ReadLine();
        }
    }
}

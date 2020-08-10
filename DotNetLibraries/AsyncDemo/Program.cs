using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            Console.WriteLine("-----开始程序-----");
            //开始计时
            sw.Start();

            //调用方法

            //普通
            //Console.WriteLine(AsyncDemo.Greeting("world"));

            //AsyncDemo.CallerWithAsync();
            //AsyncDemo.CallerWithContinuationTask();

            //多个异步方法
            //AsyncDemo.MultipleAsyncMehtods();   //两个都结束才返回
            //它可以实现一个异步方法依赖另一个异步方法的结果的情况

            //AsyncDemo.MultipleAsyncMethodsWithCombinators1();   //和上面一样
            //AsyncDemo.MultipleAsyncMethodsWithCombinators2();   //和上面一样

            //转换异步模式
            //AsyncDemo.ConvertingAsyncPattern();

            //错误处理
            //AsyncDemo.DontHandle();
            //AsyncDemo.DontHandle2();

            //AsyncDemo.StartTwoTask();
            //AsyncDemo.StartTwoTaskParallel();
            AsyncDemo.ShowAggregatedException();


            Console.WriteLine("总执行时间：" + sw.Elapsed.Seconds + "秒");
            sw.Stop();

            Console.WriteLine("-----结束程序-----");
            Console.Read();
        }
    }
}

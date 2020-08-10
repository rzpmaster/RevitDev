using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDemo
{
    class AsyncDemo
    {
        // 同步

        public static string Greeting(string name)
        {
            //等待3秒
            Task.Delay(3000).Wait(); //Wait方法用来等待之前的任务完成
            return "Hello," + name;
        }

        //异步

        public async static void CallerWithAsync()
        {
            string result = await GreetingAsync("异步调用方法");
            Console.WriteLine(result);

            // await 该方法内的方法，会等待执行
            // asyn 标记的整个方法 会直接执行，不会阻塞
        }

        private static Task<string> GreetingAsync(string name)
        {
            return Task.Run<string>(() => { return Greeting(name); });
        }

        // await 关键字的实质 
        // Task类的ContinueWith()方法定义了任务完成后就调用的代码。指派给ContinueWith()方法的委托接收将已完成的任务作为参数传入，使用Result属性可以访问任务返回的结果
        public static void CallerWithContinuationTask()
        {
            // 不使用 await 关键字，返回的时Task<T>，使用后直接返回了返回值 ，而 await 后面的方法被写入 ContinueWith() 中了
            // await 实质就是 编译器 将代码转换了 ！

            Task<string> t1 = GreetingAsync("异步调用方法");
            t1.ContinueWith(t =>
            {
                string result = t.Result;
                Console.WriteLine(result);
            });
        }

        // 使用多个异步方法

        public async static void MultipleAsyncMehtods()
        {
            string s1 = await GreetingAsync("Mul1");
            string s2 = await GreetingAsync("Mul2");
            Console.WriteLine("Mul:" + s1 + " " + s2);
        }

        public async static void MultipleAsyncMethodsWithCombinators1()
        {
            Task<string> t1 = GreetingAsync("mulA");
            Task<string> t2 = GreetingAsync("mulB");
            await Task.WhenAll(t1, t2);
            Console.WriteLine("结果：" + t1.Result + "  " + t2.Result);
        }

        public async static void MultipleAsyncMethodsWithCombinators2()
        {
            Task<string> t1 = GreetingAsync("mulA");
            Task<string> t2 = GreetingAsync("mulB");
            string[] result = await Task.WhenAll(t1, t2);   //只有返回相同的类型才能这样使用
            Console.WriteLine("结果:" + result[0] + "  " + result[1]);
        }

        // 转换异步模式

        // 依然使用上面的例子
        //public static string Greeting(string name)
        //{
        //    //等待3秒
        //    Task.Delay(3000).Wait(); //Wait方法用来等待之前的任务完成
        //    return "Hello," + name;
        //}

        //定义一个委托
        private static Func<string, string> greetingInvoker = Greeting;
        //模拟异步模式
        private static IAsyncResult BeginGreeting(
            string name, AsyncCallback callback, object state)
        {
            return greetingInvoker.BeginInvoke(name, callback, state);
        }
        //该方法返回来自于Greeting的结果
        private static string EndGreeting(IAsyncResult ar)
        {
            return greetingInvoker.EndInvoke(ar);
        }
        //使用新的基于任务的异步模式进行调用
        public static async void ConvertingAsyncPattern()
        {
            string s = await Task<string>.Factory.FromAsync<string>(
                BeginGreeting, EndGreeting, "测试", null);
            Console.WriteLine(s);
        }

        // 错误处理

        //注：该方法不是最终解决方案，终极方法见下述说明
        public static async void ThrowAfter(int ms, string message)
        {
            await Task.Delay(ms);
            throw new Exception(message);
        }
        //捕捉不到
        public static void DontHandle()
        {
            try
            {
                ThrowAfter(200, "first");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //重构版本
        //注：终极方法（返回值为Task，交给后面await）
        public async static Task ThrowAfter2(int ms, string message)
        {
            Console.WriteLine(message);
            await Task.Delay(ms);
            throw new Exception(message);
        }
        //可以捕捉到
        public async static void DontHandle2()
        {
            try
            {
                await ThrowAfter2(200, "first");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //多个异常捕捉
        //注：只能捕捉到第一个
        public static async void StartTwoTask()
        {
            try
            {
                await ThrowAfter2(2000, "first");
                //或直接条过第二步执行
                await ThrowAfter2(1000, "second");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        //注：只能捕捉到第一个
        public async static void StartTwoTaskParallel()
        {
            try
            {
                // 只能捕捉到first
                Task t1 = ThrowAfter2(2000, "first");
                // 第二部也会被执行，应为使用了Task.WhenAll()
                Task t2 = ThrowAfter2(1000, "second");
                await Task.WhenAll(t1, t2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        //注：全部捕获
        public static async void ShowAggregatedException()
        {
            Task taskResult = null;
            try
            {
                Task t1 = ThrowAfter2(2000, "first");
                Task t2 = ThrowAfter2(1000, "second");
                await (taskResult = Task.WhenAll(t1, t2));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                foreach (var ex1 in taskResult.Exception.InnerExceptions)
                {
                    Console.WriteLine(ex1.Message);
                }
            }
        }
    }
}

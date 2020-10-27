using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Log4NetDemo.Configration;
using Log4NetDemo.Core;
//using log4net;
//using log4net.Appender;
//using log4net.Config;
//using log4net.Core;
//using log4net.Layout;

namespace ConfigurationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            #region <1>手写配置
            //// 1. To create a layout
            //// 2. Use this layout in an appender
            //// 3. Initialize the configuration
            //// 4. Get the instance of the logger

            //var patterLayout = new PatternLayout();
            //patterLayout.ConversionPattern = "%data-[%level]-%message%newline";
            //patterLayout.ActivateOptions();

            //var consoleAppender = new ConsoleAppender()
            //{
            //    Name = "ConsoleAppender",
            //    Layout = patterLayout,
            //    Threshold = Level.Info
            //};
            //consoleAppender.ActivateOptions();

            //var fileAppender = new FileAppender()
            //{
            //    Name = "FileAppender",
            //    Layout = patterLayout,
            //    Threshold = Level.All,

            //    AppendToFile = true,   // 是否追加写入
            //    File = "log.log",

            //};
            //fileAppender.ActivateOptions();

            //var rollingAppender = new RollingFileAppender()
            //{
            //    Name = "RollingAppender",
            //    Layout = patterLayout,
            //    Threshold = Level.All,

            //    AppendToFile = true,
            //    File = "rollinglog.log",
            //    MaximumFileSize = "200KB",
            //    MaxSizeRollBackups = 15,
            //};
            //rollingAppender.ActivateOptions();

            //BasicConfigurator.Configure(consoleAppender, fileAppender, rollingAppender);

            //ILog log = LogManager.GetLogger(typeof(Program));

            //for (int i = 0; i < 3000; i++)
            //{
            //    log.Debug("this is a Debug infomation");
            //    log.Info("this is a Info infomation");
            //    log.Warn("this is a Warn infomation");
            //    log.Error("this is a Error infomation");
            //    log.Fatal("this is a Fatal infomation");
            //}

            //log.Debug("this is a Debug infomation");
            //log.Info("this is a Info infomation");
            //log.Warn("this is a Warn infomation");
            //log.Error("this is a Error infomation");
            //log.Fatal("this is a Fatal infomation"); 
            #endregion

            #region <2>app.congig配置
            //XmlConfigurator.Configure();// 很重要！不配置的话，拿不到app.config中配置的信息
            //ILog log = LogManager.GetLogger(typeof(Program));

            //log.Debug("this is a Debug infomation");
            //log.Info("this is a Info infomation");
            //log.Warn("this is a Warn infomation");
            //log.Error("this is a Error infomation");
            //log.Fatal("this is a Fatal infomation");

            #endregion

            #region <3>Demo

            //XmlConfigurator.Configure();// 很重要！不配置的话，拿不到app.config中配置的信息
            XmlConfigurator.Configure(new FileInfo("Log4NetDemo.config"));
            ILog log = LogManager.GetLogger(typeof(Program));

            log.Debug("this is a Debug infomation");
            log.Info("this is a Info infomation");
            log.Warn("this is a Warn infomation");
            log.Error("this is a Error infomation");
            log.Fatal("this is a Fatal infomation");

            #endregion

            Console.ReadLine();
        }
    }
}

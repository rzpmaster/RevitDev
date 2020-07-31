using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json.NetDemo
{
    class DebuggingWithSerializationTracing
    {
        // Debugging serialization using MemoryTraceWriter

        public void Test()
        {
            Product p2 = new Product
            {
                Name = "Product 2",
                Price = 12.50m,
                ExpiryDate = new DateTime(2009, 7, 31, 0, 0, 0, DateTimeKind.Utc),
            };

            ITraceWriter traceWriter = new MemoryTraceWriter();

            JsonConvert.SerializeObject(
                p2,
                new JsonSerializerSettings { TraceWriter = traceWriter, Converters = { new JavaScriptDateTimeConverter() } });

            Console.WriteLine(traceWriter);

            //2020 - 07 - 31T15: 42:15.527 Info Started serializing Json.NetDemo.Product.Path ''.
            //2020 - 07 - 31T15: 42:15.551 Info Started serializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter.Path 'ExpiryDate'.
            //2020 - 07 - 31T15: 42:15.556 Info Finished serializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter.Path 'ExpiryDate'.
            //2020 - 07 - 31T15: 42:15.565 Info Finished serializing Json.NetDemo.Product.Path ''.
            //2020 - 07 - 31T15: 42:15.565 Verbose Serialized JSON:
            //            {
            //                "Name": "Product 2",
            //                "ExpiryDate": new Date(
            //                      1248998400000
            //                              ),
            //                "Price": 12.50,
            //                 "Sizes": null
            //            }
        }
    }

    // 实现 ITraceWriter 接口，可以做一个自己的日志类记录序列化

    //public class NLogTraceWriter : ITraceWriter
    //{
    //    private static readonly Logger Logger = LogManager.GetLogger("NLogTraceWriter");

    //    public TraceLevel LevelFilter
    //    {
    //        // trace all messages. nlog can handle filtering
    //        get { return TraceLevel.Verbose; }
    //    }

    //    public void Trace(TraceLevel level, string message, Exception ex)
    //    {
    //        LogEventInfo logEvent = new LogEventInfo
    //        {
    //            Message = message,
    //            Level = GetLogLevel(level),
    //            Exception = ex
    //        };

    //        // log Json.NET message to NLog
    //        Logger.Log(logEvent);
    //    }

    //    private LogLevel GetLogLevel(TraceLevel level)
    //    {
    //        switch (level)
    //        {
    //            case TraceLevel.Error:
    //                return LogLevel.Error;
    //            case TraceLevel.Warning:
    //                return LogLevel.Warn;
    //            case TraceLevel.Info:
    //                return LogLevel.Info;
    //            case TraceLevel.Off:
    //                return LogLevel.Off;
    //            default:
    //                return LogLevel.Trace;
    //        }
    //    }
    //}
}

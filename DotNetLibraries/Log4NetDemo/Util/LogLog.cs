using System;
using System.Collections;
using System.Diagnostics;

namespace Log4NetDemo.Util
{
    public delegate void LogReceivedEventHandler(object source, LogReceivedEventArgs e);

    public class LogReceivedEventArgs : EventArgs
    {
        public LogReceivedEventArgs(LogLog loglog)
        {
            this.loglog = loglog;
        }

        public LogLog LogLog
        {
            get { return loglog; }
        }

        private readonly LogLog loglog;
    }

    public class LogLog
    {
        public LogLog(Type source, string prefix, string message, Exception exception)
        {
            timeStampUtc = DateTime.UtcNow;

            this.source = source;
            this.prefix = prefix;
            this.message = message;
            this.exception = exception;
        }

        static LogLog()
        {
            try
            {
                // 读取配置
                InternalDebugging = OptionConverter.ToBoolean(SystemInfo.GetAppSetting("log4net.Internal.Debug"), false);
                QuietMode = OptionConverter.ToBoolean(SystemInfo.GetAppSetting("log4net.Internal.Quiet"), false);
                EmitInternalMessages = OptionConverter.ToBoolean(SystemInfo.GetAppSetting("log4net.Internal.Emit"), true);
            }
            catch (Exception ex)
            {
                // If an exception is thrown here then it looks like the config file does not
                // parse correctly.
                //
                // We will leave debug OFF and print an Error message
                Error(typeof(LogLog), "Exception while reading ConfigurationSettings. Check your .config file is well formed XML.", ex);
            }
        }

        #region Public Static Event

        /// <summary>
        /// 接受到消息时触发
        /// </summary>
        public static event LogReceivedEventHandler LogReceived;

        /// <summary>
        /// 当接受到消息是触发
        /// </summary>
        /// <param name="source"></param>
        /// <param name="prefix"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void OnLogReceived(Type source, string prefix, string message, Exception exception)
        {
            LogReceived?.Invoke(null, new LogReceivedEventArgs(new LogLog(source, prefix, message, exception)));
        }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// 是否开启 Debug 等级的日志
        /// </summary>
        public static bool InternalDebugging
        {
            get { return s_debugEnabled; }
            set { s_debugEnabled = value; }
        }

        /// <summary>
        /// 是否静默模式，如果为<value>true</value>,将不会记录任何消息
        /// </summary>
        public static bool QuietMode
        {
            get { return s_quietMode; }
            set { s_quietMode = value; }
        }

        /// <summary>
        /// 是否控制台输出日志消息
        /// </summary>
        public static bool EmitInternalMessages
        {
            get { return s_emitInternalMessages; }
            set { s_emitInternalMessages = value; }
        }

        #endregion

        #region private Static Fields

        /// <summary>
        /// 是否开启 Debug 等级的日志
        /// </summary>
        private static bool s_debugEnabled = false;
        /// <summary>
        /// 是否静默模式，如果为<value>true</value>,将不会记录任何消息
        /// </summary>
        private static bool s_quietMode = false;
        /// <summary>
        /// 是否在控制台上输出日志消息
        /// </summary>
        private static bool s_emitInternalMessages = true;

        private const string PREFIX = "log4net: ";
        private const string ERR_PREFIX = "log4net:ERROR ";
        private const string WARN_PREFIX = "log4net:WARN ";

        #endregion

        #region Public Static Mehtods

        public static bool IsDebugEnabled
        {
            get { return s_debugEnabled && !s_quietMode; }
        }

        public static void Debug(Type source, string message)
        {
            if (IsDebugEnabled)
            {
                if (EmitInternalMessages)
                {
                    EmitOutLine(PREFIX + message);
                }

                OnLogReceived(source, PREFIX, message, null);
            }
        }

        public static void Debug(Type source, string message, Exception exception)
        {
            if (IsDebugEnabled)
            {
                if (EmitInternalMessages)
                {
                    EmitOutLine(PREFIX + message);
                    if (exception != null)
                    {
                        EmitOutLine(exception.ToString());
                    }
                }

                OnLogReceived(source, PREFIX, message, exception);
            }
        }

        public static bool IsWarnEnabled
        {
            get { return !s_quietMode; }
        }

        public static void Warn(Type source, string message)
        {
            if (IsWarnEnabled)
            {
                if (EmitInternalMessages)
                {
                    EmitErrorLine(WARN_PREFIX + message);
                }

                OnLogReceived(source, WARN_PREFIX, message, null);
            }
        }

        public static void Warn(Type source, string message, Exception exception)
        {
            if (IsWarnEnabled)
            {
                if (EmitInternalMessages)
                {
                    EmitErrorLine(WARN_PREFIX + message);
                    if (exception != null)
                    {
                        EmitErrorLine(exception.ToString());
                    }
                }

                OnLogReceived(source, WARN_PREFIX, message, exception);
            }
        }

        public static bool IsErrorEnabled
        {
            get { return !s_quietMode; }
        }

        public static void Error(Type source, string message)
        {
            if (IsErrorEnabled)
            {
                if (EmitInternalMessages)
                {
                    EmitErrorLine(ERR_PREFIX + message);
                }

                OnLogReceived(source, ERR_PREFIX, message, null);
            }
        }

        public static void Error(Type source, string message, Exception exception)
        {
            if (IsErrorEnabled)
            {
                if (EmitInternalMessages)
                {
                    EmitErrorLine(ERR_PREFIX + message);
                    if (exception != null)
                    {
                        EmitErrorLine(exception.ToString());
                    }
                }

                OnLogReceived(source, ERR_PREFIX, message, exception);
            }
        }

        #endregion

        #region private Static Methods

        private static void EmitOutLine(string message)
        {
            try
            {
                Console.Out.WriteLine(message);
                Trace.WriteLine(message);
            }
            catch
            {
                // Ignore exception, what else can we do? Not really a good idea to propagate back to the caller
            }
        }

        private static void EmitErrorLine(string message)
        {
            try
            {
                Console.Error.WriteLine(message);
                Trace.WriteLine(message);
            }
            catch
            {
                // Ignore exception, what else can we do? Not really a good idea to propagate back to the caller
            }
        }

        #endregion

        #region Public Instance Properties

        public Type Source
        {
            get { return source; }
        }

        public DateTime TimeStamp
        {
            get { return timeStampUtc.ToLocalTime(); }
        }

        public DateTime TimeStampUtc
        {
            get { return timeStampUtc; }
        }

        /// <summary>
        /// 消息前缀
        /// </summary>
        public string Prefix
        {
            get { return prefix; }
        }

        public string Message
        {
            get { return message; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Optional. Will be null if no Exception was passed.
        /// </remarks>
        public Exception Exception
        {
            get { return exception; }
        }

        #endregion

        #region parvate Instance Fields

        private readonly Type source;
        private readonly DateTime timeStampUtc;
        private readonly string prefix;
        private readonly string message;
        private readonly Exception exception;

        #endregion

        public override string ToString()
        {
            return Prefix + Source.Name + ": " + Message;
        }

        /// <summary>
        /// 订阅事件，并存储消息
        /// </summary>
        public class LogReceivedAdapter : IDisposable
        {
            private readonly IList items;

            public LogReceivedAdapter(IList items)
            {
                this.items = items;

                LogReceived += LogLog_LogReceived;
            }

            void LogLog_LogReceived(object source, LogReceivedEventArgs e)
            {
                items.Add(e.LogLog);
            }

            /// <summary>
            /// 存储 的（订阅事件后发生的）日志消息
            /// </summary>
            public IList Items
            {
                get { return items; }
            }

            public void Dispose()
            {
                LogReceived -= LogLog_LogReceived;
            }
        }
    }
}

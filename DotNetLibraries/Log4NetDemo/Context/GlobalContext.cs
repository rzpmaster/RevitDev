using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;

namespace Log4NetDemo.Context
{
    /// <summary>
    /// 全局上下文
    /// </summary>
    public sealed class GlobalContext
    {
        private GlobalContext()
        {
        }

        static GlobalContext()
        {
            Properties[LoggingEvent.HostNameProperty] = SystemInfo.HostName;
        }

        private readonly static GlobalContextProperties s_properties = new GlobalContextProperties();
        /// <summary>
        /// 全局上下文属性字典
        /// </summary>
        public static GlobalContextProperties Properties
        {
            get { return s_properties; }
        }
    }
}
